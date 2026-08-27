# Nexus Mods Full Download Integration — Implementation Guide

## Purpose

This document is a complete implementation guide for adding one-click Nexus Mods download/update support to RHI. It covers what already exists in RHI, the Nexus API in full, and exactly what needs to be built — file by file.

---

## Build & Test Commands

```powershell
# Build
dotnet build g:\RDXC\RenoDXCommander\RenoDXCommander.csproj --no-restore -v q -p:Platform=x64

# Publish (deploys to the running build location)
# Run publish.bat from g:\RDXC\
```

---

## What Already Exists in RHI

### Update Detection — `NexusUpdateService.cs`
`RenoDXCommander/Services/NexusUpdateService.cs`

Already working. Uses the **Nexus GraphQL v2 API** (`https://api.nexusmods.com/v2/graphql`) — **no API key required**.

- Queries `legacyModsByDomain` with `{gameDomain, modId}` pairs
- `ParseNexusUrl(url)` → extracts `(Domain, ModId)` from a `nexusmods.com/domain/mods/id` URL
- Compares `updatedAt` timestamp against persisted baselines in `%LocalAppData%\RHI\nexus_baselines.json`
- `NexusBaseline` model: `Domain`, `ModId`, `LastKnownUpdate`, `InstalledVersion`, `HasUpdate`
- Flags `card.Status = UpdateAvailable` when remote is newer

**Missing from `NexusBaseline`:** `FileId` (the specific file_id that was installed). Needs adding.

### Game Catalogue — `NexusModsService.cs`
`RenoDXCommander/Services/NexusModsService.cs`

Fetches Nexus game catalogue (`https://data.nexusmods.com/file/nexus-data/games.json`), builds a normalised lookup of game name → Nexus URL. Used for the PCGW-style wiki link button, NOT for downloads. This service is separate from download functionality.

### Card State for Nexus Mods
A card gets `IsExternalOnly = true` when `effectiveMod.SnapshotUrl == null && effectiveMod.NexusUrl != null`. These are mods hosted on Nexus that RHI can't currently download directly.

When `IsExternalOnly`:
- `card.ExternalUrl` = the Nexus mod page URL (e.g. `https://www.nexusmods.com/kingdomsofamalurreckoning/mods/64`)
- `card.ExternalLabel` = `"Download from Nexus Mods"`
- The install button opens the browser to `ExternalUrl` (see `MainWindow.Events.cs` → `ExternalLinkButton_Click`)
- `card.NexusUrl` = same as `ExternalUrl` for Nexus mods (set in `MainViewModel.Install.cs` and `BuildCards.cs`)
- `AutoUpdateService` currently **excludes** `IsExternalOnly` cards — line 113: `&& !c.IsExternalOnly`

### Existing Install Record — `InstalledModRecord.cs`
`RenoDXCommander/Models/InstalledModRecord.cs`

Fields: `GameName`, `Store`, `AddonFileName`, `FileHash`, `InstalledAt`, `InstallPath`, `InstalledVersion`, plus others.

**Missing:** `NexusFileId` (int) — the file_id from Nexus API, needed to identify which file is installed vs latest. Add this field.

### Single Instance Forwarding — `SingleInstanceService.cs`
`RenoDXCommander/Services/SingleInstanceService.cs`

Uses a named pipe (`RenoDXCommander_AddonPipe`) to forward file paths from a second instance to the running one. `FileReceived` event fires with the path. This exact pattern is needed for NXM protocol forwarding — the NXM URL arrives as a command-line arg to a new RHI process that needs to forward it to the running instance.

### Settings Persistence — `SettingsViewModel.cs`
`RenoDXCommander/ViewModels/SettingsViewModel.cs`

Uses `[ObservableProperty]` fields with `LoadSettingsFromDict` / `SaveSettingsToDict` pattern against `%LocalAppData%\RHI\settings.json`. Add `NexusApiKey` (string) and `NexusIsPremium` (bool) here.

Pattern for a new string setting:
```csharp
// Declaration
[ObservableProperty] private string _nexusApiKey = "";

// Load
if (s.TryGetValue("NexusApiKey", out var nakVal)) NexusApiKey = nakVal ?? "";

// Save
if (!string.IsNullOrEmpty(NexusApiKey)) s["NexusApiKey"] = NexusApiKey;
```

---

## Nexus Mods API — Complete Reference

### Base URL
```
https://api.nexusmods.com/v1/
```

### Authentication — All v1 REST Endpoints
Every request needs these headers:
```
apikey: {USER_API_KEY}
Application-Name: RHI
Application-Version: 2.4.x
```

### Validate API Key + Get Membership Status
```
GET /v1/users/validate.json
```
Returns:
```json
{
  "user_id": 12345,
  "key": "...",
  "name": "Username",
  "is_premium": true,
  "is_supporter": false
}
```
Call this when the user enters/connects their key. Store `is_premium` as `NexusIsPremium` in settings.

### Get Mod Files List
```
GET /v1/games/{game_domain}/mods/{mod_id}/files.json
```
Returns all files for the mod. Each file has:
```json
{
  "file_id": 67890,
  "name": "Mod Name",
  "version": "1.5",
  "category_name": "MAIN",
  "is_primary": true,
  "uploaded_timestamp": 1700000000,
  "size_kb": 1234,
  "file_name": "modname-1.5.zip"
}
```
To find the latest file to download: filter `category_name == "MAIN"`, pick highest `uploaded_timestamp`.

### Get Download Links (PREMIUM ONLY)
```
GET /v1/games/{game_domain}/mods/{mod_id}/files/{file_id}/download_links.json
```
**Returns HTTP 403 for free users** with message: `"You don't have permission to get download links from the API without visiting nexusmods.com — this is for premium users only."`

For premium users, returns:
```json
[
  {
    "name": "Nexus CDN",
    "short_name": "Nexus",
    "URI": "https://cf-files.nexusmods.com/cdn/...?md5=xxx&expires=1234567890&user_id=xxx"
  }
]
```
The `URI` is a direct HTTPS download link that **expires in ~30 minutes**. Never cache it — generate fresh per download.

### Get Download Links (FREE — NXM Key Path)
Same endpoint but with query params from the NXM protocol URL:
```
GET /v1/games/{game_domain}/mods/{mod_id}/files/{file_id}/download_links.json?key={key}&expires={expires}&user_id={user_id}
```
This works for free users when the params come from an NXM link generated by the Nexus website.

### Rate Limits
- 20,000 requests per 24 hours (resets 00:00 GMT)
- After 20k: 500 per hour
- Response headers: `X-RL-Hourly-Remaining`, `X-RL-Daily-Remaining`, `X-RL-Hourly-Reset`, `X-RL-Daily-Reset`
- Always check these headers and back off gracefully

---

## NXM Protocol — Free User Download Path

### URL Format
```
nxm://{game_domain}/mods/{mod_id}/files/{file_id}?key={key}&expires={timestamp}&user_id={user_id}
```
Example:
```
nxm://kingdomsofamalurreckoning/mods/64/files/67890?key=AbCdEf123&expires=1787000000&user_id=99999
```

### How It Works
1. User clicks "Mod Manager Download" on nexusmods.com
2. Browser fires the `nxm://` protocol, Windows looks up the registered handler
3. The registered handler (RHI) receives the URL as a command-line arg: `RHI.exe --nxm "nxm://..."`
4. RHI parses the URL, calls the download_links endpoint with the key params
5. Downloads and installs the file

### Windows Registry — Protocol Handler
Write at RHI first launch or in installer:
```
HKEY_CURRENT_USER\Software\Classes\nxm
  (Default) = "URL:NXM Protocol"
  "URL Protocol" = ""

HKEY_CURRENT_USER\Software\Classes\nxm\shell\open\command
  (Default) = "\"C:\Users\...\RHI.exe\" --nxm \"%1\""
```
Use the `Environment.ProcessPath` for the RHI exe path (not `AppContext.BaseDirectory` — wrong for single-file publish).

**Conflict with Vortex/MO2**: These apps also register `nxm://`. Whichever registered last wins. Only overwrite if no handler exists, or show a dialog offering to claim it.

### Single Instance Forwarding for NXM
RHI may already be running when the NXM URL arrives. The new process must forward the URL to the running instance:

In `App.OnLaunched`, check for `--nxm` arg. If RHI is already running, call `SingleInstanceService.SendToRunningInstance("nxm:" + url)` (use a prefix to distinguish from addon files) and exit.

In the running instance, `SingleInstanceService.FileReceived` fires. Add a check: if the received string starts with `"nxm:"`, route to the NXM handler instead of the addon drag-drop handler.

`SingleInstanceService` pipe name: `RenoDXCommander_AddonPipe`. The existing pipe is string-based — just prefix the NXM URL with `"nxm:"` so the receiver can distinguish.

---

## App Registration with Nexus (Required for Public Release)

Before shipping to users, email `support@nexusmods.com` with:
- A testing build of RHI demonstrating API key input
- App name: `RHI`
- Short description: tool for managing ReShade, RenoDX and HDR mods across PC game libraries
- Logo: high-res, visible on dark background

They assign a **slug** (e.g. `"rhi"`) used for SSO. Until registered, personal API keys work for testing. Using personal keys for a public app violates their AUP.

### SSO Flow (After Registration — Optional but Better UX)
Instead of copy-paste, users can authorise via browser:
1. Generate UUID v4
2. Open WebSocket: `wss://sso.nexusmods.com`
3. Send: `{ "id": "<uuid>", "appid": "rhi" }`
4. Ping every 30s to keep alive
5. Open `https://www.nexusmods.com/sso?id=<uuid>` in the user's browser
6. User clicks Authorise on the Nexus site
7. WebSocket receives the API key as a plain string
8. Save key, close socket

WinUI 3 `Windows.System.Launcher.LaunchUriAsync` opens the browser. For the WebSocket, use `System.Net.WebSockets.ClientWebSocket`.

---

## What Needs to Be Built — File by File

### 1. `SettingsViewModel.cs` — Add API Key Fields
```csharp
[ObservableProperty] private string _nexusApiKey = "";
[ObservableProperty] private bool _nexusIsPremium;
[ObservableProperty] private string _nexusUsername = "";
```
Load/save following existing pattern. Never log the API key value.

### 2. `NexusDownloadService.cs` (new file)
`RenoDXCommander/Services/NexusDownloadService.cs`

```csharp
public class NexusDownloadService
{
    Task<NexusUserInfo?> ValidateApiKeyAsync(string apiKey);
    Task<List<NexusModFile>> GetModFilesAsync(string domain, int modId);
    Task<string?> GetDownloadUriAsync(string domain, int modId, int fileId);        // premium
    Task<string?> GetDownloadUriWithNxmKeyAsync(string domain, int modId, int fileId, string key, string expires, string userId); // free
    Task<string?> DownloadToTempAsync(string uri, IProgress<(string msg, double pct)>? progress);
    bool IsApiKeyConfigured { get; }
    bool IsPremium { get; }
}
```

Models:
```csharp
public record NexusUserInfo(int UserId, string Name, bool IsPremium);
public record NexusModFile(int FileId, string Name, string Version, string CategoryName, long UploadedTimestamp, string FileName);
```

Inject via DI as singleton. Register in `App.xaml.cs` alongside other services.

### 3. `InstalledModRecord.cs` — Add NexusFileId
Add:
```csharp
public int? NexusFileId { get; set; }
```
Write this when installing a Nexus mod (drag-drop today, direct download later).

### 4. `NexusBaseline` — Add FileId
In `NexusUpdateService.cs`, add to `NexusBaseline`:
```csharp
[JsonPropertyName("fileId")]
public int? FileId { get; set; }
```

### 5. Settings UI
In `MainWindow.xaml`, add a new section in the relevant settings card (or create a "Nexus Mods" card). Needs:
- A `PasswordBox` (or `TextBox`) for API key paste
- A "Connect" button that calls `ValidateApiKeyAsync` and shows `"Connected as {name} (Premium)"` or `"Connected as {name} (Free)"`
- A "Disconnect" button that clears the key
- Description text explaining premium = automatic downloads, free = one-click via browser

In `SettingsHandler.cs`, initialize the UI from `ViewModel.Settings.NexusApiKey` in `SettingsButton_Click`.

### 6. `NxmProtocolHandler.cs` (new file)
`RenoDXCommander/Services/NxmProtocolHandler.cs`

Handles parsing and routing of NXM URLs:
```csharp
public static class NxmProtocolHandler
{
    // Parses nxm://domain/mods/id/files/fileid?key=x&expires=y&user_id=z
    public static NxmLink? Parse(string nxmUrl);

    // Writes the registry entries to claim the nxm:// handler
    public static void RegisterProtocolHandler();

    // Returns true if RHI is currently the registered nxm:// handler
    public static bool IsRegistered();
}

public record NxmLink(string Domain, int ModId, int FileId, string Key, string Expires, string UserId);
```

Call `RegisterProtocolHandler()` on first launch (check `IsRegistered()` first to avoid overwriting Vortex).

### 7. `App.OnLaunched` — NXM Argument Handling
In `App.xaml.cs`, `OnLaunched` already checks for `--nxm` pattern from command-line args. Add:
```csharp
string? nxmArg = null;
if (cmdArgs.Length > 1 && cmdArgs[1].StartsWith("nxm://", StringComparison.OrdinalIgnoreCase))
    nxmArg = cmdArgs[1];
// OR --nxm "nxm://..." (two-arg form for protocol handler)
var nxmIdx = Array.IndexOf(cmdArgs, "--nxm");
if (nxmIdx >= 0 && nxmIdx < cmdArgs.Length - 1)
    nxmArg = cmdArgs[nxmIdx + 1];
```

If RHI is already running: `SingleInstanceService.SendToRunningInstance("nxm:" + nxmArg)` and exit.
If this is the first instance: store the NXM URL, process it after `MainWindow` is ready.

### 8. `SingleInstanceService.cs` — NXM Routing
In `MainWindow.xaml.cs`, where `FileReceived` is wired:
```csharp
SingleInstanceService.FileReceived += path =>
{
    if (path.StartsWith("nxm:", StringComparison.OrdinalIgnoreCase))
        HandleIncomingNxmUrl(path.Substring(4));
    else
        HandleIncomingAddonFile(path);
};
```

`HandleIncomingNxmUrl` parses the NXM link, calls `NexusDownloadService.GetDownloadUriWithNxmKeyAsync`, downloads, then routes to the install flow.

### 9. `MainViewModel.Install.Nexus.cs` (new partial file)
`RenoDXCommander/ViewModels/MainViewModel.Install.Nexus.cs`

```csharp
public partial class MainViewModel
{
    // Downloads and installs a specific Nexus file for a card
    public async Task InstallNexusModAsync(GameCardViewModel card, int fileId);

    // Finds the latest MAIN file and installs it (update path)
    public async Task UpdateNexusModAsync(GameCardViewModel card);

    // Handles an incoming NXM link — finds the matching card and installs
    public async Task HandleNxmLinkAsync(NxmLink link);
}
```

`UpdateNexusModAsync` flow:
1. Parse `card.NexusUrl` via `NexusUpdateService.ParseNexusUrl()` → get `(domain, modId)`
2. Call `NexusDownloadService.GetModFilesAsync(domain, modId)` → get files list
3. Find latest MAIN file (highest `UploadedTimestamp` where `CategoryName == "MAIN"`)
4. If `NexusIsPremium`: call `GetDownloadUriAsync` → download → extract/deploy
5. If free: open browser to mod page (existing behaviour) — NXM path handles the rest when user clicks
6. On success: update `InstalledModRecord.NexusFileId`, call `NexusUpdateService.ResetBaseline(card.GameName)`

### 10. `AutoUpdateService.cs` — Add Nexus Cards
In `RunUpdatePassAsync`, remove `&& !c.IsExternalOnly` filter and replace with Nexus-specific handling:
```csharp
// Nexus mods — premium only for silent update
var nexusCards = cards.Where(c =>
    c.Status == GameStatus.UpdateAvailable
    && !c.IsHidden
    && !c.ExcludeFromUpdateAllRenoDx
    && c.IsExternalOnly
    && !string.IsNullOrEmpty(c.NexusUrl)
    && _nexusDownloadService.IsPremium
    && !string.IsNullOrEmpty(c.InstallPath)).ToList();

foreach (var card in nexusCards)
{
    if (card.IsRunning) { EnqueueRetry(card, "Nexus"); continue; }
    await TryUpdateOneAsync("Nexus", card, () => _viewModel!.UpdateNexusModAsync(card));
    await Pause();
}
```

`AutoUpdateService` currently doesn't have access to `NexusDownloadService` — inject it via `SetViewModel` or add it to the constructor.

### 11. Install/Update Button Routing
In `MainWindow.Events.cs`, `ExternalLinkButton_Click` currently always opens the browser. When a Nexus API key is configured AND user is premium, intercept:

```csharp
private async void ExternalLinkButton_Click(object sender, RoutedEventArgs e)
{
    var card = GetCardFromSender(sender);
    if (card == null) return;

    var nexusService = App.Services.GetRequiredService<NexusDownloadService>();
    if (card.NexusUrl != null && nexusService.IsApiKeyConfigured && nexusService.IsPremium)
    {
        // Download directly
        await ViewModel.UpdateNexusModAsync(card);
        return;
    }

    // Existing browser-open path
    var url = card.IsExternalOnly ? card.ExternalUrl : (card.NexusUrl ?? card.DiscordUrl ?? card.ExternalUrl);
    if (!string.IsNullOrEmpty(url))
        await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
    ...
}
```

---

## Extraction / Install After Download — Exact Pattern

Once the CDN URI is obtained, the download and install must go through the existing infrastructure, not a new pipeline. Here is the exact flow:

### The Nexus download is a `.zip` containing a `.addon64` file

This is the standard case for Nexus-hosted RenoDX mods. The flow:

1. Download the zip to a temp path using `HttpClient` (stream to file, same pattern as `ModInstallService.InstallAsync` steps 3–4)
2. Extract using 7-Zip: `App.Services.GetRequiredService<ISevenZipExtractor>().Find7ZipExe()` to get the 7z exe, then `Process.Start` with args `x "{zipPath}" -o"{tempDir}" -y`
3. Find `.addon64`/`.addon32` files in the extracted temp dir:
   ```csharp
   var addonFiles = Directory.GetFiles(tempDir, "*.addon64", SearchOption.AllDirectories)
       .Concat(Directory.GetFiles(tempDir, "*.addon32", SearchOption.AllDirectories))
       .Where(f => Path.GetFileName(f).StartsWith("renodx-", StringComparison.OrdinalIgnoreCase))
       .ToList();
   ```
4. Copy the addon to the game's deploy path:
   ```csharp
   var deployDir = ModInstallService.GetAddonDeployPath(card.InstallPath);
   File.Copy(addonPath, Path.Combine(deployDir, addonFileName), overwrite: true);
   ```
5. Save an `InstalledModRecord`:
   ```csharp
   var record = new InstalledModRecord
   {
       GameName = card.GameName,
       Store = card.Source ?? "",
       InstallPath = card.InstallPath,
       AddonFileName = addonFileName,
       InstalledAt = DateTime.UtcNow,
       InstalledVersion = versionFromNexus,
       NexusFileId = fileId,  // new field — see Phase 3
   };
   _installer.SaveRecordPublic(record);
   ```
6. Post-install steps (mirror `UpdateOrchestrationService.UpdateAllRenoDxAsync`):
   - Deploy Engine.ini LUT: `AuxInstallService.ApplyEngineIniLutSetting(...)` if UE game
   - Deploy Engine.ini HDR: `AuxInstallService.ApplyEngineIniHdrSettings(...)` if UE-Extended
   - Apply `renodxIniOverrides` from manifest: `AuxInstallService.ApplyRenodxIniOverrides(...)`
7. Update card state on dispatcher:
   ```csharp
   DispatcherQueue?.TryEnqueue(() =>
   {
       card.InstalledRecord = record;
       card.InstalledAddonFileName = addonFileName;
       card.RdxInstalledVersion = AuxInstallService.ReadInstalledVersion(record.InstallPath, record.AddonFileName);
       card.Status = GameStatus.Installed;
       card.ActionMessage = "✅ Updated!";
       card.NotifyAll();
       card.FadeMessage(m => card.ActionMessage = m, card.ActionMessage);
   });
   ```
8. Reset Nexus baseline: `_nexusUpdateService.ResetBaseline(card.GameName)`
9. Save library: `SaveLibrary()`

**Do NOT route through `DragDropHandler.ProcessDroppedAddon`** — that method shows a game-picker dialog asking the user which game to install to. For programmatic install where you already know the card, copy the file and save the record directly as above.

`DragDropHandler.ProcessDroppedArchive` and `ProcessDroppedAddon` are for **user-initiated drag-drop only** — they show ContentDialogs, ask for confirmation, and require user interaction. The download path must be silent.

### Deploy path helper
```csharp
ModInstallService.GetAddonDeployPath(card.InstallPath)
// Returns the addon subfolder if the game uses one, otherwise the install path itself
```

---

## Card State — `InstallActionLabel`, `CanInstall`, `CardRdxInstallEnabled`

These are computed properties in `GameCardViewModel.RenoDX.cs`.

**Current state for `IsExternalOnly` cards** (Nexus-only mods today):
- `CanInstall` returns `false` — it explicitly excludes `IsExternalOnly`: `Mod?.SnapshotUrl != null && !IsInstalling && !IsExternalOnly && ...`
- `InstallActionLabel` falls through to the Status-based labels (Install/Update/Reinstall) but the button is disabled
- The card row shows an external link button instead of the install button

**What needs to change** when adding direct download support:

`CanInstall` in `GameCardViewModel.RenoDX.cs` line 30 needs to include the Nexus premium path:
```csharp
public bool CanInstall => IsRtxHdrEnabled
    || (Mod?.SnapshotUrl != null && !IsInstalling && !IsExternalOnly && (IsRsInstalled || ExcludeFromUpdateAllReShade))
    || (IsExternalOnly && NexusUrl != null && /* nexusDownloadService.IsPremium — pass via card property */);
```

The cleanest approach: add `[ObservableProperty] private bool _nexusDirectDownloadAvailable` to `GameCardViewModel.cs`, set it in `BuildCards`/`CacheLoad` when `card.NexusUrl != null && settings.NexusIsPremium`, and use it in the `CanInstall` expression. This avoids service dependencies in the ViewModel.

`InstallActionLabel` for external-only with direct download:
- `Status == NotInstalled` → `"Download from Nexus Mods"` (or `"Install"`)
- `Status == UpdateAvailable` → `"⬆  Update"` 
- `Status == Installed` → `"↺  Reinstall"`

The existing `ExternalLink_Click` handler in `MainWindow.Events.cs` (line 683) is what fires for the external link button. That's the interception point for premium users — add the service check there.

---

## `GameMod.NexusUrl` — Where It Comes From

**Read-only. Do not write to it programmatically.**

`WikiService.cs` scrapes the RenoDX wiki mod table. For each mod row, it scans all links — if a link contains `nexusmods.com`, it's stored as `nexusUrl`. This becomes `GameMod.NexusUrl`. Set in `WikiService.FetchAllAsync()` line 233.

The manifest `nexusUrlOverrides` dict can override this per game — checked in `NexusModsService.ResolveUrl()`, but that's for the **PCGW-style info button link**, not the mod download URL.

For the download feature, `card.NexusUrl` (from `GameMod.NexusUrl`) is the mod page URL, from which `NexusUpdateService.ParseNexusUrl()` extracts `(Domain, ModId)`. You then call the files API to find the latest file_id. **The wiki `NexusUrl` is the mod page — not a direct download link.**

---

## `CheckInstallWarningAsync` — Call Pattern

**Defined in:** `MainViewModel.Install.Luma.cs` line 1016

```csharp
public async Task<bool> CheckInstallWarningAsync(string gameName, string component)
```

- `component` is a string key matching entries in `manifest.InstallWarnings` dict (e.g. `"renodx"`, `"reshade"`, `"luma"`, `"dxvk"`, etc.)
- Returns `true` → proceed with install
- Returns `false` → user cancelled, abort install
- Shows a `ContentDialog` with the manifest-defined warning message if one exists for this game+component combo

**Call it in `InstallNexusModAsync` before starting the download:**
```csharp
if (!await CheckInstallWarningAsync(card.GameName, "renodx")) return;
```

Use the `"renodx"` key since Nexus mods are RenoDX addons. No new component key needed.

---

## `ExternalLink_Click` — Current Implementation

`MainWindow.Events.cs` line 683:
```csharp
internal async void ExternalLink_Click(object sender, RoutedEventArgs e)
{
    var card = GetCardFromSender(sender);
    if (card == null) return;

    var url = card.IsExternalOnly ? card.ExternalUrl : (card.NexusUrl ?? card.DiscordUrl ?? card.ExternalUrl);

    if (!string.IsNullOrEmpty(url))
        await Windows.System.Launcher.LaunchUriAsync(new Uri(url));

    // Reset Nexus baseline when user acknowledges the update
    if (card.Status == GameStatus.UpdateAvailable && card.IsExternalOnly)
    {
        var nexusService = App.Services.GetRequiredService<INexusUpdateService>();
        nexusService.ResetBaseline(card.GameName);
        card.Status = GameStatus.Installed;
    }
}
```

**Intercept point for Phase 2** — before the `LaunchUriAsync` call, check:
```csharp
var nexusDownload = App.Services.GetRequiredService<NexusDownloadService>();
if (card.NexusUrl != null && nexusDownload.IsApiKeyConfigured && nexusDownload.IsPremium)
{
    await ViewModel.InstallNexusModAsync(card);
    return;
}
// fall through to browser open
```


---

## Implementation Order

| Phase | Files | User benefit |
|---|---|---|
| 1 | `SettingsViewModel` + `NexusDownloadService` (validate only) + Settings UI | Users can connect account, see premium status |
| 2 | `GetModFilesAsync` + `GetDownloadUriAsync` + `DownloadToTempAsync` + `InstallNexusModAsync` + button routing | Premium users get one-click download/update from the card |
| 3 | `InstalledModRecord.NexusFileId` + `NexusBaseline.FileId` + record on install | Accurate update detection — know which file is installed |
| 4 | `UpdateNexusModAsync` + `AutoUpdateService` Nexus cards | Premium users get silent auto-update |
| 5 | `NxmProtocolHandler` registration + `App.OnLaunched` NXM parsing + `SingleInstanceService` routing | Free users get one-click from browser |
| 6 | SSO login flow (after Nexus app registration) | Better UX than copy-paste |

---

## Known Gotchas

- **CDN URLs expire (~30 min)** — generate fresh per download, never cache.
- **File categories**: always pick `category_name == "MAIN"` with highest `uploaded_timestamp`. Never auto-install `OLD_VERSION` or `OPTIONAL` files.
- **`content_preview_link` is broken** as of June 2026 — field in `/v1/.../files.json` returns 404 for files uploaded after ~June 11 2026. Do not use this field.
- **NXM handler conflict**: if Vortex/MO2 is installed, they're already the NXM handler. Only register if no other handler exists. Or offer a setting.
- **Single instance + NXM**: the second RHI instance has no `DispatcherQueue` yet — use `SingleInstanceService.SendToRunningInstance("nxm:" + url)` then `Environment.Exit(0)` immediately. Don't initialise the full app.
- **API key security**: never log the key value. Store it in `settings.json` as plaintext (same as other settings) — it's a local user file. Don't add extra encryption complexity.
- **`IsExternalOnly` flag**: this is set for mods where `SnapshotUrl == null && NexusUrl != null`. The install button currently opens a browser. After Phase 2, this button should download directly for premium users. The card's `ExternalLabel` / `InstallActionLabel` / `CanInstall` logic may need adjustment to show "Download" vs "Update" correctly.
- **Adult content flag**: some mods require the account's adult content setting enabled. API returns 403 for these if not enabled. Handle with a clear error message.
- **Rate limit headers**: always read from every response. If `X-RL-Daily-Remaining < 100`, back off and log a warning.
- **`NexusModsUrl` vs `NexusUrl`**: `card.NexusModsUrl` (set by `NexusModsService`) is the PCGW-style wiki link button — unrelated to downloads. `card.NexusUrl` (set from `GameMod.NexusUrl`) is the mod page URL used for downloading. Don't confuse them.

---

## Summary Table

| Capability | Premium user with API key | Free user (NXM registered) | Free user (no NXM) |
|---|---|---|---|
| Detect updates | ✅ already works | ✅ already works | ✅ already works |
| One-click install/update | ✅ Phase 2 | ✅ Phase 5 (browser click) | ❌ manual |
| Silent auto-update | ✅ Phase 4 | ❌ | ❌ |
| No browser needed | ✅ | ❌ | ❌ |
