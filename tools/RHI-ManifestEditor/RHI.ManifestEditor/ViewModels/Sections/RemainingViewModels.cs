using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

// ── DXVK ──────────────────────────────────────────────────────────────────────
public partial class DxvkSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<StringItem> DxvkBlacklist { get; }
    public ObservableCollection<GameNoteItem> DxvkGameNotes { get; }

    public DxvkSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        DxvkBlacklist = ToObservable(manifest.DxvkBlacklist);
        DxvkGameNotes = new(manifest.DxvkGameNotes?.Select(kv => new GameNoteItem(kv.Key, kv.Value))
            ?? Enumerable.Empty<GameNoteItem>());
    }

    [RelayCommand] public void AddDxvkBlacklist() { DxvkBlacklist.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveDxvkBlacklist(StringItem item) { DxvkBlacklist.Remove(item); Dirty(); }
    [RelayCommand] public void AddDxvkNote() { DxvkGameNotes.Add(new GameNoteItem()); Dirty(); }
    [RelayCommand] public void RemoveDxvkNote(GameNoteItem item) { DxvkGameNotes.Remove(item); Dirty(); }

    public override void Commit()
    {
        _manifest.DxvkBlacklist = FromObservable(DxvkBlacklist).Count > 0 ? FromObservable(DxvkBlacklist) : null;
        _manifest.DxvkGameNotes = DxvkGameNotes.Count > 0
            ? DxvkGameNotes.Where(i => !string.IsNullOrWhiteSpace(i.GameName))
                .ToDictionary(i => i.GameName, i => i.ToEntry())
            : null;
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
public partial class NvidiaSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<KeyValueItem> ProfileNameOverrides { get; }
    public ObservableCollection<StringItem> ProfileExeExclusions { get; }

    public NvidiaSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        ProfileNameOverrides = ToKvObservable(manifest.ProfileNameOverrides);
        ProfileExeExclusions = ToObservable(manifest.ProfileExeExclusions);
    }

    [RelayCommand] public void AddProfileOverride() { ProfileNameOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveProfileOverride(KeyValueItem item) { ProfileNameOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddExeExclusion() { ProfileExeExclusions.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveExeExclusion(StringItem item) { ProfileExeExclusions.Remove(item); Dirty(); }

    public override void Commit()
    {
        _manifest.ProfileNameOverrides = FromKvObservable(ProfileNameOverrides);
        _manifest.ProfileExeExclusions = FromObservable(ProfileExeExclusions).Count > 0 ? FromObservable(ProfileExeExclusions) : null;
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
