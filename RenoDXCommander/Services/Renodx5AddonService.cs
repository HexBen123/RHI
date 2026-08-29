using System.Text.Json;
using RenoDXCommander.Models;

namespace RenoDXCommander.Services;

/// <summary>
/// Manages the RenoDX DLSS5 addon — download, staging, install, and auto-update.
/// Hosted on RankFTW/rhi-repo GitHub releases under the renodx-dlss5- tag prefix.
/// Auto-redeploys to any game folder where the addon is already present.
/// </summary>
public class Renodx5AddonService
{
    private const string StagedFileName   = "renodx-dlss5.addon64";
    private const string DeployFileName   = "renodx-dlss5.addon64";
    private const string TagPrefix        = "renodx-dlss5-";
    private static readonly string GitHubApiUrl =
        "https://api.github.com/repos/RankFTW/rhi-repo/releases?per_page=100";

    private readonly HttpClient _http;
    private readonly ICrashReporter _crashReporter;
    private readonly IGameLibraryService _gameLibraryService;
    private readonly IDlssStreamlineService _dlssStreamlineService;

    private readonly string _stagingDir;
    private readonly string _versionFile;

    public Renodx5AddonService(
        HttpClient http,
        ICrashReporter crashReporter,
        IGameLibraryService gameLibraryService,
        IDlssStreamlineService dlssStreamlineService)
    {
        _http                  = http;
        _crashReporter         = crashReporter;
        _gameLibraryService    = gameLibraryService;
        _dlssStreamlineService = dlssStreamlineService;

        _stagingDir  = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RHI", "rdx5");
        _versionFile = Path.Combine(_stagingDir, "version.txt");
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Whether renodx-dlss5.addon64 is ready in staging.</summary>
    public bool IsStagingReady => File.Exists(Path.Combine(_stagingDir, StagedFileName));

    /// <summary>Full path to the staged renodx-dlss5.addon64 file.</summary>
    public string StagedFilePath => Path.Combine(_stagingDir, StagedFileName);

    /// <summary>The currently staged version string (tag suffix), or null if not staged.</summary>
    public string? StagedVersion
        => File.Exists(_versionFile) ? File.ReadAllText(_versionFile).Trim() : null;

    /// <summary>Whether an update is available (set after CheckForUpdateAsync).</summary>
    public bool HasUpdate { get; private set; }

    /// <summary>The latest remote version string (set after CheckForUpdateAsync).</summary>
    public string? LatestVersion { get; private set; }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Checks GitHub for a newer version than what's currently staged.</summary>
    public async Task<bool> CheckForUpdateAsync()
    {
        var (version, _) = await FetchLatestReleaseInfoAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(version))
        {
            _crashReporter.Log("[Renodx5AddonService.CheckForUpdateAsync] Could not resolve latest version");
            return false;
        }

        LatestVersion = version;
        var current = StagedVersion;
        HasUpdate = !string.Equals(current, version, StringComparison.OrdinalIgnoreCase);
        _crashReporter.Log($"[Renodx5AddonService.CheckForUpdateAsync] Cached={current ?? "(none)"}, Remote={version}, HasUpdate={HasUpdate}");
        return HasUpdate;
    }

    /// <summary>
    /// Ensures renodx-dlss5.addon64 is staged. Downloads if not present or if an update is available.
    /// After a successful download, auto-redeploys to all game folders that already have the addon.
    /// </summary>
    public async Task EnsureStagingAsync(IProgress<(string message, double percent)>? progress = null)
    {
        if (IsStagingReady && !HasUpdate)
        {
            _crashReporter.Log("[Renodx5AddonService.EnsureStagingAsync] Staging already valid — skipping download");
            return;
        }

        Directory.CreateDirectory(_stagingDir);
        progress?.Report(("Downloading RenoDX DLSS5 addon...", 10));

        var (version, downloadUrl) = await FetchLatestReleaseInfoAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(downloadUrl))
        {
            _crashReporter.Log("[Renodx5AddonService.EnsureStagingAsync] Could not resolve latest release");
            return;
        }

        progress?.Report(("Downloading RenoDX DLSS5 addon...", 30));

        try
        {
            var destPath = Path.Combine(_stagingDir, StagedFileName);
            var bytes    = await _http.GetByteArrayAsync(downloadUrl).ConfigureAwait(false);

            // If the download is a zip, extract the addon64 from it
            if (downloadUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var tempZip = Path.Combine(_stagingDir, "_download.zip.tmp");
                await File.WriteAllBytesAsync(tempZip, bytes).ConfigureAwait(false);
                using (var zip = System.IO.Compression.ZipFile.OpenRead(tempZip))
                {
                    var entry = zip.Entries.FirstOrDefault(e =>
                        string.Equals(e.Name, StagedFileName, StringComparison.OrdinalIgnoreCase));
                    if (entry == null)
                    {
                        _crashReporter.Log($"[Renodx5AddonService.EnsureStagingAsync] '{StagedFileName}' not found in zip");
                        File.Delete(tempZip);
                        return;
                    }
                    using var entryStream = entry.Open();
                    using var outStream   = File.Create(destPath);
                    await entryStream.CopyToAsync(outStream).ConfigureAwait(false);
                }
                File.Delete(tempZip);
            }
            else
            {
                await File.WriteAllBytesAsync(destPath, bytes).ConfigureAwait(false);
            }

            File.WriteAllText(_versionFile, version);
            HasUpdate = false;
            _crashReporter.Log($"[Renodx5AddonService.EnsureStagingAsync] Staged v{version} ({new FileInfo(destPath).Length} bytes)");
        }
        catch (Exception ex)
        {
            _crashReporter.Log($"[Renodx5AddonService.EnsureStagingAsync] Download failed ({downloadUrl}) — {ex.Message}");
            progress?.Report(($"RenoDX DLSS5 addon download failed: {ex.Message}", 0));
            return;
        }

        progress?.Report(("RenoDX DLSS5 addon ready", 90));

        // ── Auto-redeploy to all games where the addon is already present ─────
        await AutoRedeployAsync().ConfigureAwait(false);

        progress?.Report(("RenoDX DLSS5 addon ready", 100));
    }

    /// <summary>
    /// Copies nvngx_dlssnr.dll to the install path root if not already present.
    /// No-op if the file already exists (preserves any version the user or Nvidia Profile section deployed).
    /// </summary>
    public async Task DeployNrDllIfAbsentAsync(string installPath)
    {
        if (string.IsNullOrEmpty(installPath)) return;
        var nrDllDest = Path.Combine(installPath, "nvngx_dlssnr.dll");
        if (File.Exists(nrDllDest))
        {
            _crashReporter.Log($"[Renodx5AddonService.DeployNrDllIfAbsentAsync] nvngx_dlssnr.dll already present at '{installPath}' — skipping");
            return;
        }
        try
        {
            var cachedNr = await _dlssStreamlineService.EnsureNewestDlssnrCachedAsync().ConfigureAwait(false);
            if (cachedNr != null)
            {
                File.Copy(cachedNr, nrDllDest, overwrite: false);
                _crashReporter.Log($"[Renodx5AddonService.DeployNrDllIfAbsentAsync] Deployed nvngx_dlssnr.dll to '{installPath}'");
            }
            else
            {
                _crashReporter.Log("[Renodx5AddonService.DeployNrDllIfAbsentAsync] nvngx_dlssnr.dll not available — skipping");
            }
        }
        catch (Exception ex)
        {
            _crashReporter.Log($"[Renodx5AddonService.DeployNrDllIfAbsentAsync] Failed for '{installPath}' — {ex.Message}");
        }
    }

    /// <summary>
    /// Copies the staged addon64 to the given game install path (respects reshade.ini AddonPath).
    /// Also deploys nvngx_dlssnr.dll to the install path root if not already present.
    /// </summary>
    public async Task InstallAsync(string installPath)
    {
        if (string.IsNullOrEmpty(installPath)) return;

        await EnsureStagingAsync().ConfigureAwait(false);
        if (!IsStagingReady)
        {
            _crashReporter.Log("[Renodx5AddonService.InstallAsync] Staging not ready — cannot install");
            return;
        }

        try
        {
            var deployDir = ModInstallService.GetAddonDeployPath(installPath);
            Directory.CreateDirectory(deployDir);
            var src  = Path.Combine(_stagingDir, StagedFileName);
            var dest = Path.Combine(deployDir, DeployFileName);
            File.Copy(src, dest, overwrite: true);
            _crashReporter.Log($"[Renodx5AddonService.InstallAsync] Deployed addon to '{deployDir}'");
        }
        catch (Exception ex)
        {
            _crashReporter.Log($"[Renodx5AddonService.InstallAsync] Addon deploy failed for '{installPath}' — {ex.Message}");
        }

        // Deploy nvngx_dlssnr.dll alongside the addon if not already present
        await DeployNrDllIfAbsentAsync(installPath).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes renodx-dlss5.addon64 from the given game install path.
    /// </summary>
    public void Uninstall(string installPath)
    {
        if (string.IsNullOrEmpty(installPath)) return;
        var deployDir = ModInstallService.GetAddonDeployPath(installPath);
        var filePath  = Path.Combine(deployDir, DeployFileName);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _crashReporter.Log($"[Renodx5AddonService.Uninstall] Removed from '{deployDir}'");
            }
        }
        catch (Exception ex)
        {
            _crashReporter.Log($"[Renodx5AddonService.Uninstall] Failed for '{installPath}' — {ex.Message}");
        }
    }

    /// <summary>Returns true if renodx-dlss5.addon64 exists at the game's deploy path.</summary>
    public bool IsInstalledIn(string installPath)
    {
        if (string.IsNullOrEmpty(installPath)) return false;
        var deployDir = ModInstallService.GetAddonDeployPath(installPath);
        return File.Exists(Path.Combine(deployDir, DeployFileName));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Copies the staged file to every game folder that already has the addon present.
    /// Only overwrites — never fresh-installs to a game that doesn't have it yet.
    /// </summary>
    private async Task AutoRedeployAsync()
    {
        try
        {
            var staged = Path.Combine(_stagingDir, StagedFileName);
            if (!File.Exists(staged)) return;

            var lib = _gameLibraryService.Load();
            if (lib == null) return;

            var allGames = lib.Games
                .Concat(lib.ManualGames)
                .Where(g => !string.IsNullOrEmpty(g.InstallPath))
                .ToList();

            foreach (var game in allGames)
            {
                try
                {
                    var deployDir = ModInstallService.GetAddonDeployPath(game.InstallPath!);
                    var dest      = Path.Combine(deployDir, DeployFileName);

                    // Also check the install path root in case addon was deployed there directly
                    var destRoot = Path.Combine(game.InstallPath!, DeployFileName);
                    bool existsInDeployDir = File.Exists(dest);
                    bool existsInRoot = !dest.Equals(destRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(destRoot);

                    if (!existsInDeployDir && !existsInRoot) continue; // only update if already present

                    var src = Path.Combine(_stagingDir, StagedFileName);
                    if (existsInDeployDir)
                    {
                        File.Copy(src, dest, overwrite: true);
                        _crashReporter.Log($"[Renodx5AddonService.AutoRedeployAsync] Updated '{game.Name}' at '{deployDir}'");
                    }
                    if (existsInRoot)
                    {
                        File.Copy(src, destRoot, overwrite: true);
                        _crashReporter.Log($"[Renodx5AddonService.AutoRedeployAsync] Updated '{game.Name}' at root '{game.InstallPath}'");
                    }
                }
                catch (Exception ex)
                {
                    _crashReporter.Log($"[Renodx5AddonService.AutoRedeployAsync] Failed for '{game.Name}' — {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _crashReporter.Log($"[Renodx5AddonService.AutoRedeployAsync] Loop failed — {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private async Task<(string? version, string? downloadUrl)> FetchLatestReleaseInfoAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GitHubApiUrl);
            request.Headers.Add("User-Agent", "RHI");
            request.Headers.Add("Accept", "application/vnd.github+json");

            using var response = await _http.SendAsync(request).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _crashReporter.Log($"[Renodx5AddonService] GitHub API returned {response.StatusCode}");
                return (null, null);
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            var candidates = new List<(string version, string downloadUrl, Version parsed)>();

            foreach (var release in doc.RootElement.EnumerateArray())
            {
                if (!release.TryGetProperty("tag_name", out var tagEl)) continue;
                var tag = tagEl.GetString();
                if (tag == null || !tag.StartsWith(TagPrefix, StringComparison.OrdinalIgnoreCase)) continue;

                var version = tag.Substring(TagPrefix.Length);

                string? downloadUrl = null;
                if (release.TryGetProperty("assets", out var assets))
                {
                    foreach (var asset in assets.EnumerateArray())
                    {
                        var assetName = asset.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
                        if (assetName == null) continue;

                        // Accept the .addon64 directly, or a zip named with the tag prefix
                        bool isAddon = string.Equals(assetName, StagedFileName, StringComparison.OrdinalIgnoreCase);
                        bool isZip   = assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                                    && assetName.StartsWith("renodx-dlss5", StringComparison.OrdinalIgnoreCase);

                        if ((isAddon || isZip) && asset.TryGetProperty("browser_download_url", out var urlEl))
                        {
                            downloadUrl = urlEl.GetString();
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(downloadUrl)) continue;

                candidates.Add(Version.TryParse(version, out var parsed)
                    ? (version, downloadUrl!, parsed)
                    : (version, downloadUrl!, new Version(0, 0)));
            }

            if (candidates.Count == 0)
            {
                _crashReporter.Log("[Renodx5AddonService] No release found with renodx-dlss5- tag");
                return (null, null);
            }

            var best = candidates.OrderByDescending(c => c.parsed).First();
            return (best.version, best.downloadUrl);
        }
        catch (Exception ex)
        {
            _crashReporter.Log($"[Renodx5AddonService] FetchLatestReleaseInfo failed — {ex.Message}");
            return (null, null);
        }
    }
}
