using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

// ── DXVK ──────────────────────────────────────────────────────────────────────
public partial class DxvkSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<StringItem> DxvkBlacklist { get; }
    public ObservableCollection<GameNoteItem> DxvkGameNotes { get; }
    public ObservableCollection<KeyValueItem> DxvkApiOverrides { get; }

    public DxvkSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        DxvkBlacklist = ToObservable(manifest.DxvkBlacklist);
        DxvkGameNotes = new(manifest.DxvkGameNotes?.Select(kv => new GameNoteItem(kv.Key, kv.Value))
            ?? Enumerable.Empty<GameNoteItem>());
        DxvkApiOverrides = ToKvObservable(manifest.DxvkApiOverrides);
    }

    [RelayCommand] public void AddDxvkBlacklist() { DxvkBlacklist.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveDxvkBlacklist(StringItem item) { DxvkBlacklist.Remove(item); Dirty(); }
    [RelayCommand] public void AddDxvkNote() { DxvkGameNotes.Add(new GameNoteItem()); Dirty(); }
    [RelayCommand] public void RemoveDxvkNote(GameNoteItem item) { DxvkGameNotes.Remove(item); Dirty(); }
    [RelayCommand] public void AddDxvkApiOverride() { DxvkApiOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveDxvkApiOverride(KeyValueItem item) { DxvkApiOverrides.Remove(item); Dirty(); }

    public override void Commit()
    {
        _manifest.DxvkBlacklist = FromObservable(DxvkBlacklist).Count > 0 ? FromObservable(DxvkBlacklist) : null;
        _manifest.DxvkGameNotes = DxvkGameNotes.Count > 0
            ? DxvkGameNotes.Where(i => !string.IsNullOrWhiteSpace(i.GameName))
                .ToDictionary(i => i.GameName, i => i.ToEntry())
            : null;
        _manifest.DxvkApiOverrides = FromKvObservable(DxvkApiOverrides);
    }
}

// ── Wiki Status ───────────────────────────────────────────────────────────────
public partial class WikiStatusSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<KeyValueItem> WikiStatusOverrides { get; }

    public WikiStatusSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
        => WikiStatusOverrides = ToKvObservable(manifest.WikiStatusOverrides);

    [RelayCommand] public void Add() { WikiStatusOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void Remove(KeyValueItem item) { WikiStatusOverrides.Remove(item); Dirty(); }

    public override void Commit()
        => _manifest.WikiStatusOverrides = FromKvObservable(WikiStatusOverrides);
}

// ── Authors & URLs ────────────────────────────────────────────────────────────
public partial class AuthorsSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<KeyValueItem> AuthorDisplayNames { get; }
    public ObservableCollection<KeyValueItem> DonationUrls { get; }
    public ObservableCollection<KeyValueItem> AuthorOverrides { get; }
    public ObservableCollection<KeyValueItem> NexusUrlOverrides { get; }
    public ObservableCollection<KeyValueItem> PcgwUrlOverrides { get; }
    public ObservableCollection<KeyValueItem> UwFixUrlOverrides { get; }
    public ObservableCollection<KeyValueItem> UltraPlusUrlOverrides { get; }
    public ObservableCollection<KeyValueItem> OptiScalerWikiNames { get; }

    public AuthorsSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        AuthorDisplayNames  = ToKvObservable(manifest.AuthorDisplayNames);
        DonationUrls        = ToKvObservable(manifest.DonationUrls);
        AuthorOverrides     = ToKvObservable(manifest.AuthorOverrides);
        NexusUrlOverrides   = ToKvObservable(manifest.NexusUrlOverrides);
        PcgwUrlOverrides    = ToKvObservable(manifest.PcgwUrlOverrides);
        UwFixUrlOverrides   = ToKvObservable(manifest.UwFixUrlOverrides);
        UltraPlusUrlOverrides = ToKvObservable(manifest.UltraPlusUrlOverrides);
        OptiScalerWikiNames = ToKvObservable(manifest.OptiScalerWikiNames);
    }

    [RelayCommand] public void AddAuthorDisplay() { AuthorDisplayNames.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveAuthorDisplay(KeyValueItem item) { AuthorDisplayNames.Remove(item); Dirty(); }
    [RelayCommand] public void AddDonation() { DonationUrls.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveDonation(KeyValueItem item) { DonationUrls.Remove(item); Dirty(); }
    [RelayCommand] public void AddAuthorOverride() { AuthorOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveAuthorOverride(KeyValueItem item) { AuthorOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddNexus() { NexusUrlOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveNexus(KeyValueItem item) { NexusUrlOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddPcgw() { PcgwUrlOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemovePcgw(KeyValueItem item) { PcgwUrlOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddOptiScalerWiki() { OptiScalerWikiNames.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveOptiScalerWiki(KeyValueItem item) { OptiScalerWikiNames.Remove(item); Dirty(); }

    public override void Commit()
    {
        _manifest.AuthorDisplayNames  = FromKvObservable(AuthorDisplayNames);
        _manifest.DonationUrls        = FromKvObservable(DonationUrls);
        _manifest.AuthorOverrides     = FromKvObservable(AuthorOverrides);
        _manifest.NexusUrlOverrides   = FromKvObservable(NexusUrlOverrides);
        _manifest.PcgwUrlOverrides    = FromKvObservable(PcgwUrlOverrides);
        _manifest.UwFixUrlOverrides   = FromKvObservable(UwFixUrlOverrides);
        _manifest.UltraPlusUrlOverrides = FromKvObservable(UltraPlusUrlOverrides);
        _manifest.OptiScalerWikiNames = FromKvObservable(OptiScalerWikiNames);
    }
}

// ── NVIDIA / DLSS ─────────────────────────────────────────────────────────────
public partial class DlssPresetItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private int _value;
    [ObservableProperty] private bool? _disabled;
    public DlssPresetItem(ManifestPresetEntry e) { _name = e.Name; _value = e.Value; _disabled = e.Disabled; }
    public DlssPresetItem() { _name = ""; }
    public ManifestPresetEntry ToEntry() => new() { Name = Name, Value = Value, Disabled = Disabled == true ? true : null };
}

public partial class NvidiaSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<KeyValueItem> ProfileNameOverrides { get; }
    public ObservableCollection<StringItem> ProfileExeExclusions { get; }
    public ObservableCollection<DlssPresetItem> DlssSrPresets { get; }
    public ObservableCollection<DlssPresetItem> DlssRrPresets { get; }
    public ObservableCollection<DlssPresetItem> DlssFgPresets { get; }
    public ObservableCollection<DlssPresetItem> DlssSrPresetsDev { get; }
    public ObservableCollection<DlssPresetItem> DlssRrPresetsDev { get; }
    public ObservableCollection<DlssPresetItem> DlssFgPresetsDev { get; }
    [ObservableProperty] private string _rtxHdrInfoUrl;

    public NvidiaSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        ProfileNameOverrides = ToKvObservable(manifest.ProfileNameOverrides);
        ProfileExeExclusions = ToObservable(manifest.ProfileExeExclusions);
        DlssSrPresets = new(manifest.DlssPresets?.Sr?.Select(e => new DlssPresetItem(e)) ?? Enumerable.Empty<DlssPresetItem>());
        DlssRrPresets = new(manifest.DlssPresets?.Rr?.Select(e => new DlssPresetItem(e)) ?? Enumerable.Empty<DlssPresetItem>());
        DlssFgPresets = new(manifest.DlssPresets?.Fg?.Select(e => new DlssPresetItem(e)) ?? Enumerable.Empty<DlssPresetItem>());
        DlssSrPresetsDev = new(manifest.DlssPresetsDev?.Sr?.Select(e => new DlssPresetItem(e)) ?? Enumerable.Empty<DlssPresetItem>());
        DlssRrPresetsDev = new(manifest.DlssPresetsDev?.Rr?.Select(e => new DlssPresetItem(e)) ?? Enumerable.Empty<DlssPresetItem>());
        DlssFgPresetsDev = new(manifest.DlssPresetsDev?.Fg?.Select(e => new DlssPresetItem(e)) ?? Enumerable.Empty<DlssPresetItem>());
        _rtxHdrInfoUrl = manifest.RtxHdrInfoUrl ?? "";
    }

    [RelayCommand] public void AddProfileOverride() { ProfileNameOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveProfileOverride(KeyValueItem item) { ProfileNameOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddExeExclusion() { ProfileExeExclusions.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveExeExclusion(StringItem item) { ProfileExeExclusions.Remove(item); Dirty(); }
    [RelayCommand] public void AddSrPreset() { DlssSrPresets.Add(new DlssPresetItem()); Dirty(); }
    [RelayCommand] public void RemoveSrPreset(DlssPresetItem item) { DlssSrPresets.Remove(item); Dirty(); }
    [RelayCommand] public void AddRrPreset() { DlssRrPresets.Add(new DlssPresetItem()); Dirty(); }
    [RelayCommand] public void RemoveRrPreset(DlssPresetItem item) { DlssRrPresets.Remove(item); Dirty(); }
    [RelayCommand] public void AddFgPreset() { DlssFgPresets.Add(new DlssPresetItem()); Dirty(); }
    [RelayCommand] public void RemoveFgPreset(DlssPresetItem item) { DlssFgPresets.Remove(item); Dirty(); }
    [RelayCommand] public void AddSrPresetDev() { DlssSrPresetsDev.Add(new DlssPresetItem()); Dirty(); }
    [RelayCommand] public void RemoveSrPresetDev(DlssPresetItem item) { DlssSrPresetsDev.Remove(item); Dirty(); }
    [RelayCommand] public void AddRrPresetDev() { DlssRrPresetsDev.Add(new DlssPresetItem()); Dirty(); }
    [RelayCommand] public void RemoveRrPresetDev(DlssPresetItem item) { DlssRrPresetsDev.Remove(item); Dirty(); }
    [RelayCommand] public void AddFgPresetDev() { DlssFgPresetsDev.Add(new DlssPresetItem()); Dirty(); }
    [RelayCommand] public void RemoveFgPresetDev(DlssPresetItem item) { DlssFgPresetsDev.Remove(item); Dirty(); }

    partial void OnRtxHdrInfoUrlChanged(string value) => Dirty();

    public override void Commit()
    {
        _manifest.ProfileNameOverrides = FromKvObservable(ProfileNameOverrides);
        _manifest.ProfileExeExclusions = FromObservable(ProfileExeExclusions).Count > 0 ? FromObservable(ProfileExeExclusions) : null;
        _manifest.RtxHdrInfoUrl = string.IsNullOrWhiteSpace(RtxHdrInfoUrl) ? null : RtxHdrInfoUrl;

        var sr = DlssSrPresets.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Select(i => i.ToEntry()).ToList();
        var rr = DlssRrPresets.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Select(i => i.ToEntry()).ToList();
        var fg = DlssFgPresets.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Select(i => i.ToEntry()).ToList();
        _manifest.DlssPresets = (sr.Count > 0 || rr.Count > 0 || fg.Count > 0)
            ? new ManifestDlssPresets { Sr = sr.Count > 0 ? sr : null, Rr = rr.Count > 0 ? rr : null, Fg = fg.Count > 0 ? fg : null }
            : null;

        var srDev = DlssSrPresetsDev.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Select(i => i.ToEntry()).ToList();
        var rrDev = DlssRrPresetsDev.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Select(i => i.ToEntry()).ToList();
        var fgDev = DlssFgPresetsDev.Where(i => !string.IsNullOrWhiteSpace(i.Name)).Select(i => i.ToEntry()).ToList();
        _manifest.DlssPresetsDev = (srDev.Count > 0 || rrDev.Count > 0 || fgDev.Count > 0)
            ? new ManifestDlssPresets { Sr = srDev.Count > 0 ? srDev : null, Rr = rrDev.Count > 0 ? rrDev : null, Fg = fgDev.Count > 0 ? fgDev : null }
            : null;
    }
}

// ── DOF Fix ───────────────────────────────────────────────────────────────────
public partial class DofFixSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<StringItem> DofFixForceGames { get; }
    public ObservableCollection<StringItem> DofFixSkipGames { get; }

    public DofFixSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        DofFixForceGames = ToObservable(manifest.DofFixForceGames);
        DofFixSkipGames  = ToObservable(manifest.DofFixSkipGames);
    }

    [RelayCommand] public void AddForce() { DofFixForceGames.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveForce(StringItem item) { DofFixForceGames.Remove(item); Dirty(); }
    [RelayCommand] public void AddSkip() { DofFixSkipGames.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveSkip(StringItem item) { DofFixSkipGames.Remove(item); Dirty(); }

    public override void Commit()
    {
        _manifest.DofFixForceGames = FromObservable(DofFixForceGames).Count > 0 ? FromObservable(DofFixForceGames) : null;
        _manifest.DofFixSkipGames  = FromObservable(DofFixSkipGames).Count > 0 ? FromObservable(DofFixSkipGames) : null;
    }
}

// ── Packs & Components ────────────────────────────────────────────────────────
public partial class PacksSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<KeyValueItem> ComponentUrls { get; }

    public PacksSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
        => ComponentUrls = ToKvObservable(manifest.ComponentUrls);

    [RelayCommand] public void AddComponentUrl() { ComponentUrls.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveComponentUrl(KeyValueItem item) { ComponentUrls.Remove(item); Dirty(); }

    public override void Commit()
        => _manifest.ComponentUrls = FromKvObservable(ComponentUrls);
}
