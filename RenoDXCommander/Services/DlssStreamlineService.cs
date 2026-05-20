using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace RenoDXCommander.Services;

/// <summary>
/// Manages DLSS and Streamline DLL detection, version swapping, backup/restore,
/// and on-demand downloading. Implemented as a partial class.
/// </summary>
public partial class DlssStreamlineService : IDlssStreamlineService
{
    // ── Constants ─────────────────────────────────────────────────────────────

    private const string DlssDllName = "nvngx_dlss.dll";
    private const string DlssdDllName = "nvngx_dlssd.dll";
    private const string DlssgDllName = "nvngx_dlssg.dll";
    private const string StreamlineIndicator = "sl.interposer.dll";
    private const string BackupExtension = ".original";

    /// <summary>Known Streamline DLL filenames.</summary>
    public static readonly string[] KnownStreamlineDlls =
    [
        "sl.common.dll",
        "sl.deepdvc.dll",
        "sl.directsr.dll",
        "sl.dlss.dll",
        "sl.dlss_d.dll",
        "sl.dlss_g.dll",
        "sl.interposer.dll",
        "sl.nis.dll",
        "sl.nvperf.dll",
        "sl.pcl.dll",
        "sl.reflex.dll",
    ];

    // ── Staging directories ───────────────────────────────────────────────────

    private static readonly string BaseStagingDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RHI");

    private static readonly string DlssCacheDir = Path.Combine(BaseStagingDir, "DLSS");
    private static readonly string DlssdCacheDir = Path.Combine(BaseStagingDir, "DLSS-D");
    private static readonly string DlssgCacheDir = Path.Combine(BaseStagingDir, "DLSS-G");
    private static readonly string StreamlineCacheDir = Path.Combine(BaseStagingDir, "Streamline");
    private static readonly string DlssCustomDir = Path.Combine(BaseStagingDir, "DLSS-Custom");
    private static readonly string StreamlineCustomDir = Path.Combine(BaseStagingDir, "Streamline-Custom");

    // ── Manifest URL ──────────────────────────────────────────────────────────

    private const string DlssManifestUrl =
        "https://raw.githubusercontent.com/RankFTW/RHI/main/dlss_manifest.json";

    private static readonly string ManifestCachePath = Path.Combine(BaseStagingDir, "dlss_manifest.json");

    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly HttpClient _http;
    private readonly GitHubETagCache _etagCache;

    // ── State ─────────────────────────────────────────────────────────────────

    private DlssManifestData? _manifest;

    public IReadOnlyList<string> DlssVersions => _manifest?.Dlss?.Select(e => e.Version).ToList().AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();
    public IReadOnlyList<string> DlssdVersions => _manifest?.Dlssd?.Select(e => e.Version).ToList().AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();
    public IReadOnlyList<string> DlssgVersions => _manifest?.Dlssg?.Select(e => e.Version).ToList().AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();
    public IReadOnlyList<string> StreamlineVersions => _manifest?.Streamline?.Select(e => e.Version).ToList().AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    public DlssStreamlineService(HttpClient http, GitHubETagCache etagCache)
    {
        _http = http;
        _etagCache = etagCache;

        // Try load cached manifest synchronously for immediate availability
        LoadCachedManifest();
    }

    // ── Manifest fetching ─────────────────────────────────────────────────────

    public async Task FetchManifestAsync()
    {
        try
        {
            var json = await _http.GetStringAsync(DlssManifestUrl).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(json))
            {
                var manifest = JsonSerializer.Deserialize<DlssManifestData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (manifest != null)
                {
                    _manifest = manifest;

                    // Cache to disk
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(ManifestCachePath)!);
                        await File.WriteAllTextAsync(ManifestCachePath, json).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        CrashReporter.Log($"[DlssStreamlineService.FetchManifestAsync] Cache write failed — {ex.Message}");
                    }
                }

                CrashReporter.Log($"[DlssStreamlineService.FetchManifestAsync] Loaded: " +
                    $"{_manifest?.Dlss?.Count ?? 0} SR, {_manifest?.Dlssd?.Count ?? 0} RR, " +
                    $"{_manifest?.Dlssg?.Count ?? 0} FG, {_manifest?.Streamline?.Count ?? 0} SL versions");
            }
        }
        catch (Exception ex)
        {
            CrashReporter.Log($"[DlssStreamlineService.FetchManifestAsync] Fetch failed — {ex.Message}");
            // Fall back to cached
            LoadCachedManifest();
        }
    }

    private void LoadCachedManifest()
    {
        try
        {
            if (File.Exists(ManifestCachePath))
            {
                var json = File.ReadAllText(ManifestCachePath);
                _manifest = JsonSerializer.Deserialize<DlssManifestData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                CrashReporter.Log($"[DlssStreamlineService.LoadCachedManifest] Loaded from cache");
            }
        }
        catch (Exception ex)
        {
            CrashReporter.Log($"[DlssStreamlineService.LoadCachedManifest] Failed — {ex.Message}");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public string? GetFileVersion(string dllPath)
    {
        try
        {
            if (!File.Exists(dllPath)) return null;
            var info = FileVersionInfo.GetVersionInfo(dllPath);
            return $"{info.FileMajorPart}.{info.FileMinorPart}.{info.FileBuildPart}.{info.FilePrivatePart}";
        }
        catch (Exception ex)
        {
            CrashReporter.Log($"[DlssStreamlineService.GetFileVersion] Failed for '{dllPath}' — {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Formats a raw 4-part version (e.g. "3.10.6.0") into display format (e.g. "310.6.0").
    /// Trims trailing .0 segments.
    /// </summary>
    public static string FormatVersion(string? rawVersion)
    {
        if (string.IsNullOrEmpty(rawVersion)) return "Unknown";

        var span = rawVersion.AsSpan();
        while (span.EndsWith(".0"))
            span = span[..^2];

        var result = span.ToString();
        return result.Contains('.') ? result : $"{result}.0";
    }

    public bool HasBackup(string dllPath) => File.Exists(dllPath + BackupExtension);

    public string? GetNewestDlssVersion() => _manifest?.Dlss?.FirstOrDefault()?.Version;

    public async Task<string?> EnsureNewestDlssCachedAsync()
    {
        var newest = _manifest?.Dlss?.FirstOrDefault();
        if (newest == null) return null;

        var cachedDir = Path.Combine(DlssCacheDir, newest.Version);
        var cachedDll = Path.Combine(cachedDir, DlssDllName);

        if (File.Exists(cachedDll))
            return cachedDll;

        // Download on-demand
        await DownloadAndCacheAsync(newest.Url, cachedDir, DlssDllName).ConfigureAwait(false);
        return File.Exists(cachedDll) ? cachedDll : null;
    }
}

// ── Manifest data model ───────────────────────────────────────────────────────

public class DlssManifestData
{
    public List<DlssManifestEntry>? Dlss { get; set; }
    public List<DlssManifestEntry>? Dlssd { get; set; }
    public List<DlssManifestEntry>? Dlssg { get; set; }
    public List<DlssManifestEntry>? Streamline { get; set; }
}

public class DlssManifestEntry
{
    public string Version { get; set; } = "";
    public string Url { get; set; } = "";
}
