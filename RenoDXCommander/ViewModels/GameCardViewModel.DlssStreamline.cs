using CommunityToolkit.Mvvm.ComponentModel;
using RenoDXCommander.Services;

namespace RenoDXCommander.ViewModels;

// DLSS & Streamline detection state stored per-game card
public partial class GameCardViewModel
{
    // ── DLSS/Streamline detection result (set during scan) ────────────────────

    /// <summary>Full detection result from the last scan. Null if not yet scanned.</summary>
    public DlssDetectionResult? DlssDetection { get; set; }

    // ── Installed versions (display) ──────────────────────────────────────────

    [ObservableProperty] private string? _dlssInstalledVersion;
    [ObservableProperty] private string? _dlssdInstalledVersion;
    [ObservableProperty] private string? _dlssgInstalledVersion;
    [ObservableProperty] private string? _streamlineInstalledVersion;

    // ── Whether each component is present in the game ─────────────────────────

    public bool HasDlss => DlssDetection?.DlssPath != null;
    public bool HasDlssd => DlssDetection?.DlssdPath != null;
    public bool HasDlssg => DlssDetection?.DlssgPath != null;
    public bool HasStreamline => DlssDetection?.StreamlineInterposerPath != null;
    public bool HasAnyDlssStreamline => DlssDetection?.HasAny ?? false;

    // ── Whether backups exist (indicates a swap was done) ─────────────────────

    public bool DlssHasBackup => DlssDetection?.DlssPath != null && File.Exists(DlssDetection.DlssPath + ".original");
    public bool DlssdHasBackup => DlssDetection?.DlssdPath != null && File.Exists(DlssDetection.DlssdPath + ".original");
    public bool DlssgHasBackup => DlssDetection?.DlssgPath != null && File.Exists(DlssDetection.DlssgPath + ".original");
    public bool StreamlineHasBackup => DlssDetection?.StreamlineFolder != null
        && Directory.Exists(DlssDetection.StreamlineFolder)
        && Directory.EnumerateFiles(DlssDetection.StreamlineFolder, "*.original").Any();

    public bool HasAnyDlssBackup => DlssHasBackup || DlssdHasBackup || DlssgHasBackup || StreamlineHasBackup;

    // ── Refresh detection state ───────────────────────────────────────────────

    /// <summary>
    /// Updates the card's DLSS/Streamline properties from a fresh detection result.
    /// </summary>
    public void ApplyDlssDetection(DlssDetectionResult detection)
    {
        DlssDetection = detection;

        DlssInstalledVersion = detection.DlssVersion != null
            ? DlssStreamlineService.FormatVersion(detection.DlssVersion) : null;
        DlssdInstalledVersion = detection.DlssdVersion != null
            ? DlssStreamlineService.FormatVersion(detection.DlssdVersion) : null;
        DlssgInstalledVersion = detection.DlssgVersion != null
            ? DlssStreamlineService.FormatVersion(detection.DlssgVersion) : null;
        StreamlineInstalledVersion = detection.StreamlineVersion != null
            ? DlssStreamlineService.FormatVersion(detection.StreamlineVersion) : null;

        NotifyDlssStreamlineDependents();
    }

    /// <summary>
    /// Re-reads versions from disk and updates the card properties.
    /// Call after a swap or restore operation.
    /// </summary>
    public void RefreshDlssVersions(IDlssStreamlineService service)
    {
        if (DlssDetection == null) return;

        if (DlssDetection.DlssPath != null)
            DlssInstalledVersion = DlssStreamlineService.FormatVersion(service.GetFileVersion(DlssDetection.DlssPath));
        if (DlssDetection.DlssdPath != null)
            DlssdInstalledVersion = DlssStreamlineService.FormatVersion(service.GetFileVersion(DlssDetection.DlssdPath));
        if (DlssDetection.DlssgPath != null)
            DlssgInstalledVersion = DlssStreamlineService.FormatVersion(service.GetFileVersion(DlssDetection.DlssgPath));
        if (DlssDetection.StreamlineInterposerPath != null)
            StreamlineInstalledVersion = DlssStreamlineService.FormatVersion(service.GetFileVersion(DlssDetection.StreamlineInterposerPath));

        NotifyDlssStreamlineDependents();
    }

    private void NotifyDlssStreamlineDependents()
    {
        OnPropertyChanged(nameof(HasDlss));
        OnPropertyChanged(nameof(HasDlssd));
        OnPropertyChanged(nameof(HasDlssg));
        OnPropertyChanged(nameof(HasStreamline));
        OnPropertyChanged(nameof(HasAnyDlssStreamline));
        OnPropertyChanged(nameof(DlssHasBackup));
        OnPropertyChanged(nameof(DlssdHasBackup));
        OnPropertyChanged(nameof(DlssgHasBackup));
        OnPropertyChanged(nameof(StreamlineHasBackup));
        OnPropertyChanged(nameof(HasAnyDlssBackup));
    }
}
