using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

public partial class EngineSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<KeyValueItem> EngineOverrides { get; }
    public ObservableCollection<KeyValueItem> EngineHintOverrides { get; }
    public ObservableCollection<KeyValueItem> EngineIniPathOverrides { get; }
    public ObservableCollection<KeyValueItem> GraphicsApiOverrides { get; }
    public ObservableCollection<StringItem> ThirtyTwoBitGames { get; }
    public ObservableCollection<StringItem> SixtyFourBitGames { get; }
    public ObservableCollection<StringItem> DlssSkipGames { get; }
    public ObservableCollection<KeyValueItem> PdUpscalerGames { get; }

    public EngineSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        EngineOverrides = ToKvObservable(manifest.EngineOverrides);
        EngineHintOverrides = ToKvObservable(manifest.EngineHintOverrides);
        EngineIniPathOverrides = ToKvObservable(manifest.EngineIniPathOverrides);
        GraphicsApiOverrides = ToKvObservable(manifest.GraphicsApiOverrides);
        ThirtyTwoBitGames = ToObservable(manifest.ThirtyTwoBitGames);
        SixtyFourBitGames = ToObservable(manifest.SixtyFourBitGames);
        DlssSkipGames = ToObservable(manifest.DlssSkipGames);
        PdUpscalerGames = ToKvObservable(manifest.PdUpscalerGames);
    }

    [RelayCommand] public void AddEngineOverride() { EngineOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveEngineOverride(KeyValueItem item) { EngineOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddEngineHintOverride() { EngineHintOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveEngineHintOverride(KeyValueItem item) { EngineHintOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddEngineIniPathOverride() { EngineIniPathOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveEngineIniPathOverride(KeyValueItem item) { EngineIniPathOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void AddGraphicsApiOverride() { GraphicsApiOverrides.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemoveGraphicsApiOverride(KeyValueItem item) { GraphicsApiOverrides.Remove(item); Dirty(); }
    [RelayCommand] public void Add32Bit() { ThirtyTwoBitGames.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void Remove32Bit(StringItem item) { ThirtyTwoBitGames.Remove(item); Dirty(); }
    [RelayCommand] public void Add64Bit() { SixtyFourBitGames.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void Remove64Bit(StringItem item) { SixtyFourBitGames.Remove(item); Dirty(); }
    [RelayCommand] public void AddDlssSkip() { DlssSkipGames.Add(new StringItem("")); Dirty(); }
    [RelayCommand] public void RemoveDlssSkip(StringItem item) { DlssSkipGames.Remove(item); Dirty(); }
    [RelayCommand] public void AddPdUpscaler() { PdUpscalerGames.Add(new KeyValueItem()); Dirty(); }
    [RelayCommand] public void RemovePdUpscaler(KeyValueItem item) { PdUpscalerGames.Remove(item); Dirty(); }

    public override void Commit()
    {
        _manifest.EngineOverrides = FromKvObservable(EngineOverrides);
        _manifest.EngineHintOverrides = FromKvObservable(EngineHintOverrides);
        _manifest.EngineIniPathOverrides = FromKvObservable(EngineIniPathOverrides);
        _manifest.GraphicsApiOverrides = FromKvObservable(GraphicsApiOverrides);
        _manifest.ThirtyTwoBitGames = FromObservable(ThirtyTwoBitGames).Count > 0 ? FromObservable(ThirtyTwoBitGames) : null;
        _manifest.SixtyFourBitGames = FromObservable(SixtyFourBitGames).Count > 0 ? FromObservable(SixtyFourBitGames) : null;
        _manifest.DlssSkipGames = FromObservable(DlssSkipGames).Count > 0 ? FromObservable(DlssSkipGames) : null;
        _manifest.PdUpscalerGames = FromKvObservable(PdUpscalerGames);    }
}
