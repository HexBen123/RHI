using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

public partial class NotesSectionViewModel : SectionViewModelBase
{
    public ObservableCollection<GameNoteItem> GameNotes { get; }
    public ObservableCollection<GameNoteItem> ReshadeGameInfo { get; }
    public ObservableCollection<GameNoteItem> RelimiterGameInfo { get; }
    public ObservableCollection<GameNoteItem> DisplayCommanderGameInfo { get; }
    public ObservableCollection<GameNoteItem> ReframeworkGameInfo { get; }
    public ObservableCollection<GameNoteItem> OptiScalerGameInfo { get; }
    public ObservableCollection<GameNoteItem> LumaGameInfo { get; }
    public ObservableCollection<GameNoteItem> LumaGameNotes { get; }

    public NotesSectionViewModel(RemoteManifest manifest, MainViewModel main) : base(manifest, main)
    {
        GameNotes           = ToNoteObservable(manifest.GameNotes);
        ReshadeGameInfo     = ToNoteObservable(manifest.ReshadeGameInfo);
        RelimiterGameInfo   = ToNoteObservable(manifest.RelimiterGameInfo);
        DisplayCommanderGameInfo = ToNoteObservable(manifest.DisplayCommanderGameInfo);
        ReframeworkGameInfo = ToNoteObservable(manifest.ReframeworkGameInfo);
        OptiScalerGameInfo  = ToNoteObservable(manifest.OptiScalerGameInfo);
        LumaGameInfo        = ToNoteObservable(manifest.LumaGameInfo);
        LumaGameNotes       = ToNoteObservable(manifest.LumaGameNotes);
    }

    private static ObservableCollection<GameNoteItem> ToNoteObservable(Dictionary<string, GameNoteEntry>? dict)
        => new(dict?.Select(kv => new GameNoteItem(kv.Key, kv.Value)) ?? Enumerable.Empty<GameNoteItem>());

    private static Dictionary<string, GameNoteEntry>? FromNoteObservable(ObservableCollection<GameNoteItem> col)
    {
        var items = col.Where(i => !string.IsNullOrWhiteSpace(i.GameName)).ToList();
        return items.Count == 0 ? null : items.ToDictionary(i => i.GameName, i => i.ToEntry());
    }

    [RelayCommand] public void AddGameNote() { GameNotes.Add(new GameNoteItem()); Dirty(); }
    [RelayCommand] public void RemoveGameNote(GameNoteItem item) { GameNotes.Remove(item); Dirty(); }
    [RelayCommand] public void AddReshadeInfo() { ReshadeGameInfo.Add(new GameNoteItem()); Dirty(); }
    [RelayCommand] public void RemoveReshadeInfo(GameNoteItem item) { ReshadeGameInfo.Remove(item); Dirty(); }
    [RelayCommand] public void AddLumaNote() { LumaGameNotes.Add(new GameNoteItem()); Dirty(); }
    [RelayCommand] public void RemoveLumaNote(GameNoteItem item) { LumaGameNotes.Remove(item); Dirty(); }

    public override void Commit()
    {
        _manifest.GameNotes                 = FromNoteObservable(GameNotes);
        _manifest.ReshadeGameInfo           = FromNoteObservable(ReshadeGameInfo);
        _manifest.RelimiterGameInfo         = FromNoteObservable(RelimiterGameInfo);
        _manifest.DisplayCommanderGameInfo  = FromNoteObservable(DisplayCommanderGameInfo);
        _manifest.ReframeworkGameInfo       = FromNoteObservable(ReframeworkGameInfo);
        _manifest.OptiScalerGameInfo        = FromNoteObservable(OptiScalerGameInfo);
        _manifest.LumaGameInfo              = FromNoteObservable(LumaGameInfo);
        _manifest.LumaGameNotes             = FromNoteObservable(LumaGameNotes);
    }
}
