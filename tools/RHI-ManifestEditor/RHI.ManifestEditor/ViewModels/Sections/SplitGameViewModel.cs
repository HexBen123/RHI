using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

/// <summary>A single sub-game entry within a split game.</summary>
public partial class SplitGameEntryItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _subPath;

    public SplitGameEntryItem(SplitGameEntry e) { _name = e.Name; _subPath = e.SubPath; }
    public SplitGameEntryItem() { _name = ""; _subPath = ""; }

    public SplitGameEntry ToEntry() => new() { Name = Name, SubPath = SubPath };
}

/// <summary>A detected game that gets split into multiple sub-game cards.</summary>
public partial class SplitGameItem : ObservableObject
{
    [ObservableProperty] private string _detectedName;
    public ObservableCollection<SplitGameEntryItem> SubGames { get; } = new();

    private readonly MainViewModel _main;

    public SplitGameItem(string detectedName, List<SplitGameEntry> entries, MainViewModel main)
    {
        _detectedName = detectedName;
        _main = main;
        foreach (var e in entries)
            SubGames.Add(new SplitGameEntryItem(e));
        SubGames.CollectionChanged += (_, _) => _main.MarkDirty();
    }

    public SplitGameItem(MainViewModel main)
    {
        _detectedName = "";
        _main = main;
        SubGames.CollectionChanged += (_, _) => _main.MarkDirty();
    }

    [RelayCommand]
    public void AddSubGame()
    {
        SubGames.Add(new SplitGameEntryItem());
        _main.MarkDirty();
    }

    [RelayCommand]
    public void RemoveSubGame(SplitGameEntryItem item)
    {
        SubGames.Remove(item);
        _main.MarkDirty();
    }
}
