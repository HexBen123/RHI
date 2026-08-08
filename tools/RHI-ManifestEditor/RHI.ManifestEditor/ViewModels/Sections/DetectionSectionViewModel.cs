using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

public partial class DetectionSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<StringItem> Blacklist { get; }
    public ObservableCollection<StringItem> WikiUnlinks { get; }
    public ObservableCollection<KeyValueItem> WikiNameOverrides { get; }
    public ObservableCollection<KeyValueItem> LumaNameOverrides { get; }
    public ObservableCollection<KeyValueItem> InstallPathOverrides { get; }

    public DetectionSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        Blacklist = ToObservable(manifest.Blacklist);
        WikiUnlinks = ToObservable(manifest.WikiUnlinks);
        WikiNameOverrides = ToKvObservable(manifest.WikiNameOverrides);
        LumaNameOverrides = ToKvObservable(manifest.LumaNameOverrides);
        InstallPathOverrides = ToKvObservable(manifest.InstallPathOverrides);
        Subscribe(Blacklist); Subscribe(WikiUnlinks);
        Subscribe(WikiNameOverrides); Subscribe(LumaNameOverrides); Subscribe(InstallPathOverrides);
    }

    private void Subscribe<T>(ObservableCollection<T> col) where T : class
        => col.CollectionChanged += (_, _) => Dirty();

    [RelayCommand] public void AddBlacklist() { Blacklist.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveBlacklist(StringItem item) { Blacklist.Remove(item); Dirty(); }
    [RelayCommand] public void AddWikiUnlink() { WikiUnlinks.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveWikiUnlink(StringItem item) { WikiUnlinks.Remove(item); Dirty(); }
    [RelayCommand] public void AddWikiNameOverride() { WikiNameOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveWikiNameOverride(KeyValueItem item) { WikiNameOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddLumaNameOverride() { LumaNameOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveLumaNameOverride(KeyValueItem item) { LumaNameOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddInstallPathOverride() { InstallPathOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveInstallPathOverride(KeyValueItem item) { InstallPathOverrides.Remove(item); Dirty(); }

    [RelayCommand] public void SortBlacklist() { var sorted = Blacklist.OrderBy(i => i.Value).ToList(); Blacklist.Clear(); foreach (var i in sorted) Blacklist.Add(i); Dirty(); }
    [RelayCommand] public void SortWikiUnlinks() { var sorted = WikiUnlinks.OrderBy(i => i.Value).ToList(); WikiUnlinks.Clear(); foreach (var i in sorted) WikiUnlinks.Add(i); Dirty(); }

    public override void Commit()
    {
        _manifest.Blacklist = FromObservable(Blacklist).Count > 0 ? FromObservable(Blacklist) : null;
        _manifest.WikiUnlinks = FromObservable(WikiUnlinks).Count > 0 ? FromObservable(WikiUnlinks) : null;
        _manifest.WikiNameOverrides = FromKvObservable(WikiNameOverrides);
        _manifest.LumaNameOverrides = FromKvObservable(LumaNameOverrides);
        _manifest.InstallPathOverrides = FromKvObservable(InstallPathOverrides);
    }
}
