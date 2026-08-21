using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

/// <summary>A single entry in emulatorGames — emulator name + list of addon names + optional addonUrls.</summary>
public partial class EmulatorGameItem : ObservableObject
{
    [ObservableProperty] private string _emulatorName;
    public ObservableCollection<StringItem> Addons { get; }
    public ObservableCollection<KeyValueItem> AddonUrls { get; }

    private readonly MainViewModel _main;

    public EmulatorGameItem(string name, EmulatorConfig cfg, MainViewModel main)
    {
        _emulatorName = name;
        _main = main;
        Addons = new(cfg.Addons.Select(a => new StringItem(a)));
        AddonUrls = new(cfg.AddonUrls?.Select(kv => new KeyValueItem(kv.Key, kv.Value)) ?? Enumerable.Empty<KeyValueItem>());
        Addons.CollectionChanged += (_, _) => _main.MarkDirty();
        AddonUrls.CollectionChanged += (_, _) => _main.MarkDirty();
    }

    public EmulatorGameItem(MainViewModel main)
    {
        _emulatorName = "";
        _main = main;
        Addons = new();
        AddonUrls = new();
        Addons.CollectionChanged += (_, _) => _main.MarkDirty();
        AddonUrls.CollectionChanged += (_, _) => _main.MarkDirty();
    }

    [RelayCommand] public void AddAddon() { Addons.Add(new StringItem("")); _main.MarkDirty(); }
    [RelayCommand] public void RemoveAddon(StringItem i) { Addons.Remove(i); _main.MarkDirty(); }
    [RelayCommand] public void AddAddonUrl() { AddonUrls.Add(new KeyValueItem()); _main.MarkDirty(); }
    [RelayCommand] public void RemoveAddonUrl(KeyValueItem i) { AddonUrls.Remove(i); _main.MarkDirty(); }

    public EmulatorConfig ToConfig() => new()
    {
        Addons = Addons.Select(a => a.Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToList(),
        AddonUrls = AddonUrls.Count > 0
            ? AddonUrls.Where(kv => !string.IsNullOrWhiteSpace(kv.Key))
                       .ToDictionary(kv => kv.Key, kv => kv.Value)
            : null
    };
}

public partial class DetectionSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<StringItem> Blacklist { get; }
    public ObservableCollection<StringItem> WikiUnlinks { get; }
    public ObservableCollection<KeyValueItem> WikiNameOverrides { get; }
    public ObservableCollection<KeyValueItem> LumaNameOverrides { get; }
    public ObservableCollection<KeyValueItem> InstallPathOverrides { get; }
    public ObservableCollection<SplitGameItem> SplitGames { get; }
    public ObservableCollection<KeyValueItem> SteamAppIdOverrides { get; }
    public ObservableCollection<EmulatorGameItem> EmulatorGames { get; }

    public DetectionSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        Blacklist = ToObservable(manifest.Blacklist);
        WikiUnlinks = ToObservable(manifest.WikiUnlinks);
        WikiNameOverrides = ToKvObservable(manifest.WikiNameOverrides);
        LumaNameOverrides = ToKvObservable(manifest.LumaNameOverrides);
        InstallPathOverrides = ToKvObservable(manifest.InstallPathOverrides);
        SplitGames = new(manifest.SplitGames?
            .Select(kv => new SplitGameItem(kv.Key, kv.Value, main)) ?? Enumerable.Empty<SplitGameItem>());
        SteamAppIdOverrides = new(manifest.SteamAppIdOverrides?
            .Select(kv => new KeyValueItem(kv.Key, kv.Value.ToString())) ?? Enumerable.Empty<KeyValueItem>());
        EmulatorGames = new(manifest.EmulatorGames?
            .Select(kv => new EmulatorGameItem(kv.Key, kv.Value, main)) ?? Enumerable.Empty<EmulatorGameItem>());
        Subscribe(Blacklist); Subscribe(WikiUnlinks);
        Subscribe(WikiNameOverrides); Subscribe(LumaNameOverrides); Subscribe(InstallPathOverrides);
        Subscribe(SteamAppIdOverrides);
        SplitGames.CollectionChanged += (_, _) => Dirty();
        EmulatorGames.CollectionChanged += (_, _) => Dirty();
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
    [RelayCommand] public void AddSplitGame() { SplitGames.Add(new SplitGameItem(_main)); Dirty(); }
    [RelayCommand] public void RemoveSplitGame(SplitGameItem item) { SplitGames.Remove(item); Dirty(); }
    [RelayCommand] public void AddSteamAppId() { SteamAppIdOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveSteamAppId(KeyValueItem item) { SteamAppIdOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddEmulatorGame() { EmulatorGames.Add(new EmulatorGameItem(_main)); Dirty(); }
    [RelayCommand] public void RemoveEmulatorGame(EmulatorGameItem item) { EmulatorGames.Remove(item); Dirty(); }

    [RelayCommand] public void SortBlacklist() { var sorted = Blacklist.OrderBy(i => i.Value).ToList(); Blacklist.Clear(); foreach (var i in sorted) Blacklist.Add(i); Dirty(); }
    [RelayCommand] public void SortWikiUnlinks() { var sorted = WikiUnlinks.OrderBy(i => i.Value).ToList(); WikiUnlinks.Clear(); foreach (var i in sorted) WikiUnlinks.Add(i); Dirty(); }

    public override void Commit()
    {
        _manifest.Blacklist = FromObservable(Blacklist).Count > 0 ? FromObservable(Blacklist) : null;
        _manifest.WikiUnlinks = FromObservable(WikiUnlinks).Count > 0 ? FromObservable(WikiUnlinks) : null;
        _manifest.WikiNameOverrides = FromKvObservable(WikiNameOverrides);
        _manifest.LumaNameOverrides = FromKvObservable(LumaNameOverrides);
        _manifest.InstallPathOverrides = FromKvObservable(InstallPathOverrides);
        _manifest.SplitGames = SplitGames.Count > 0
            ? SplitGames
                .Where(g => !string.IsNullOrWhiteSpace(g.DetectedName))
                .ToDictionary(g => g.DetectedName, g => g.SubGames
                    .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                    .Select(s => s.ToEntry())
                    .ToList())
            : null;
        _manifest.SteamAppIdOverrides = SteamAppIdOverrides.Count > 0
            ? SteamAppIdOverrides
                .Where(i => !string.IsNullOrWhiteSpace(i.Key) && int.TryParse(i.Value, out _))
                .ToDictionary(i => i.Key, i => int.Parse(i.Value))
            : null;
        _manifest.EmulatorGames = EmulatorGames.Count > 0
            ? EmulatorGames
                .Where(g => !string.IsNullOrWhiteSpace(g.EmulatorName))
                .ToDictionary(g => g.EmulatorName, g => g.ToConfig())
            : null;
    }
}
