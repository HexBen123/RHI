namespace RenoDXCommander.Services;

public partial class OptiScalerService
{
    // ── Streamline deployment ─────────────────────────────────────────────────

    /// <summary>
    /// The Streamline staging folder — %LocalAppData%\RHI\Streamline\.
    /// Mirrors the private StreamlineCacheDir in DlssStreamlineService.
    /// </summary>
    public static string StreamlineStagingFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RHI", "Streamline");

    /// <summary>
    /// Copies all files from the RHI Streamline staging folder to
    /// &lt;installPath&gt;\OptiScaler\Streamline\.
    /// </summary>
    public void DeployStreamlineToGame(string installPath)
    {
        if (string.IsNullOrEmpty(installPath)) return;

        var sourceRootDir = StreamlineStagingFolder;
        if (!Directory.Exists(sourceRootDir))
        {
            CrashReporter.Log($"[OptiScalerService.DeployStreamlineToGame] Streamline staging folder not found — {sourceRootDir}");
            return;
        }

        // Streamline files live in a version subfolder (e.g. Streamline\2.12.0\)
        // Find the first subdirectory that contains .dll files
        var sourceDir = sourceRootDir;
        var subDirs = Directory.GetDirectories(sourceRootDir);
        if (subDirs.Length > 0)
        {
            // Use the subfolder if it contains DLLs (version folder pattern)
            var candidate = subDirs[0];
            if (Directory.GetFiles(candidate, "*.dll").Length > 0)
                sourceDir = candidate;
        }

        var destDir = Path.Combine(installPath, "OptiScaler", "Streamline");
        Directory.CreateDirectory(destDir);

        var files = Directory.GetFiles(sourceDir, "*.dll");
        int copied = 0;
        foreach (var file in files)
        {
            try
            {
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
                copied++;
            }
            catch (Exception ex)
            {
                CrashReporter.Log($"[OptiScalerService.DeployStreamlineToGame] Failed to copy '{Path.GetFileName(file)}' — {ex.Message}");
            }
        }

        CrashReporter.Log($"[OptiScalerService.DeployStreamlineToGame] Deployed {copied} Streamline file(s) to {installPath}");
    }

    /// <summary>
    /// Removes the OptiScaler\Streamline\ subfolder from the given game install path.
    /// </summary>
    public void RemoveStreamlineFromGame(string installPath)
    {
        if (string.IsNullOrEmpty(installPath)) return;

        var destDir = Path.Combine(installPath, "OptiScaler", "Streamline");
        if (Directory.Exists(destDir))
        {
            try
            {
                Directory.Delete(destDir, recursive: true);
                CrashReporter.Log($"[OptiScalerService.RemoveStreamlineFromGame] Removed Streamline folder from {installPath}");
            }
            catch (Exception ex)
            {
                CrashReporter.Log($"[OptiScalerService.RemoveStreamlineFromGame] Failed — {ex.Message}");
            }
        }
    }
}
