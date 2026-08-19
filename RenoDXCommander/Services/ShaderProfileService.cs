using System.IO.Compression;
using System.Text.Json;
using RenoDXCommander.Models;

namespace RenoDXCommander.Services;

/// <summary>
/// Loads, saves, and exports shader profiles from/to local app data.
/// Profiles are global (not per-game).
/// </summary>
public static class ShaderProfileService
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RHI", "shader_profiles.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Loads all saved shader profiles from disk.
    /// Returns an empty list on missing or corrupt file.
    /// </summary>
    public static List<ShaderProfile> Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var loaded = JsonSerializer.Deserialize<List<ShaderProfile>>(json);
                if (loaded != null)
                {
                    CrashReporter.Log($"[ShaderProfileService.Load] Loaded {loaded.Count} profile(s)");
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            CrashReporter.Log($"[ShaderProfileService.Load] Failed to load profiles: {ex.Message}");
        }
        return new List<ShaderProfile>();
    }

    /// <summary>
    /// Saves all shader profiles to disk. Creates the directory if needed.
    /// </summary>
    public static void Save(List<ShaderProfile> profiles)
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(profiles, JsonOptions);
            File.WriteAllText(FilePath, json);
            CrashReporter.Log($"[ShaderProfileService.Save] Saved {profiles.Count} profile(s)");
        }
        catch (Exception ex)
        {
            CrashReporter.Log($"[ShaderProfileService.Save] Failed to save profiles: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds a zip of the staged .fx/.fxh/texture files for the given selection (respecting exclusions).
    /// Returns the path to the temp zip file.
    /// </summary>
    public static string BuildExportZip(
        List<string> selectedPackIds,
        Dictionary<string, HashSet<string>> fileExclusions,
        IShaderPackService shaderPackService)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var zipPath = Path.Combine(Path.GetTempPath(), $"RHI_Shaders_{timestamp}.zip");

        if (File.Exists(zipPath)) File.Delete(zipPath);

        // Build a temp staging dir of selected files, then zip it
        var tempDir = Path.Combine(Path.GetTempPath(), $"RHI_Shaders_stage_{timestamp}");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        Directory.CreateDirectory(tempDir);

        try
        {
            var shadersOut = Path.Combine(tempDir, "Shaders");
            var texturesOut = Path.Combine(tempDir, "Textures");
            Directory.CreateDirectory(shadersOut);
            Directory.CreateDirectory(texturesOut);

            var shadersDir  = ShaderPackService.ShadersDir;
            var texturesDir = ShaderPackService.TexturesDir;

            foreach (var packId in selectedPackIds)
            {
                fileExclusions.TryGetValue(packId, out var excluded);

                // Shaders: walk the pack's subfolder in staging, preserving relative paths
                var packShadersDir = Path.Combine(shadersDir, packId);
                if (Directory.Exists(packShadersDir))
                {
                    foreach (var srcPath in Directory.EnumerateFiles(packShadersDir, "*", SearchOption.AllDirectories))
                    {
                        var leafName = Path.GetFileName(srcPath);
                        if (excluded != null && excluded.Contains(leafName)) continue;

                        // Preserve subfolder structure relative to the pack root
                        var relPath = Path.GetRelativePath(packShadersDir, srcPath);
                        var destPath = Path.Combine(shadersOut, packId, relPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        File.Copy(srcPath, destPath, overwrite: true);
                    }
                }

                // Textures: same pattern
                var packTexturesDir = Path.Combine(texturesDir, packId);
                if (Directory.Exists(packTexturesDir))
                {
                    foreach (var srcPath in Directory.EnumerateFiles(packTexturesDir, "*", SearchOption.AllDirectories))
                    {
                        var relPath  = Path.GetRelativePath(packTexturesDir, srcPath);
                        var destPath = Path.Combine(texturesOut, packId, relPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        File.Copy(srcPath, destPath, overwrite: true);
                    }
                }
            }

            // Also include the ReShade framework headers from the staging root (ReShade.fxh, ReShadeUI.fxh)
            foreach (var header in Directory.EnumerateFiles(shadersDir, "*.fxh", SearchOption.TopDirectoryOnly))
            {
                var destPath = Path.Combine(shadersOut, Path.GetFileName(header));
                File.Copy(header, destPath, overwrite: true);
            }

            ZipFile.CreateFromDirectory(tempDir, zipPath);
            CrashReporter.Log($"[ShaderProfileService.BuildExportZip] Created zip: {zipPath}");
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { /* best-effort cleanup */ }
        }

        return zipPath;
    }
}
