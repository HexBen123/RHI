using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;

namespace RHI.ManifestEditor.ViewModels;

/// <summary>Base for all section ViewModels. Provides helpers for list and dict editing.</summary>
public abstract partial class SectionViewModelBase : ObservableObject
{
    protected readonly RemoteManifest _manifest;
    public readonly MainViewModel _main;

    protected SectionViewModelBase(RemoteManifest manifest, MainViewModel main)
    {
        _manifest = manifest;
        _main = main;
    }

    protected void Dirty() => _main.MarkDirty();

    /// <summary>Commits in-memory edits back to the manifest model. Called before save.</summary>
    public abstract void Commit();

    // ── List helpers ──────────────────────────────────────────────────────────

    protected static ObservableCollection<StringItem> ToObservable(List<string>? list)
        => new(list?.Select(s => new StringItem(s)) ?? Enumerable.Empty<StringItem>());

    protected static List<string> FromObservable(ObservableCollection<StringItem> col)
        => col.Select(i => i.Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();

    // ── Dict helpers ──────────────────────────────────────────────────────────

    protected static ObservableCollection<KeyValueItem> ToKvObservable(Dictionary<string, string>? dict)
        => new(dict?.Select(kv => new KeyValueItem(kv.Key, kv.Value)) ?? Enumerable.Empty<KeyValueItem>());

    protected static Dictionary<string, string>? FromKvObservable(ObservableCollection<KeyValueItem> col)
    {
        var items = col.Where(i => !string.IsNullOrWhiteSpace(i.Key)).ToList();
        return items.Count == 0 ? null : items.ToDictionary(i => i.Key, i => i.Value);
    }
}

/// <summary>A single editable string item (for list sections).</summary>
public partial class StringItem : ObservableObject
{
    [ObservableProperty] private string _value;
    public StringItem(string value) => _value = value;
}

/// <summary>A key-value pair item (for dict sections).</summary>
public partial class KeyValueItem : ObservableObject
{
    [ObservableProperty] private string _key;
    [ObservableProperty] private string _value;
    public KeyValueItem(string key, string value) { _key = key; _value = value; }
    public KeyValueItem() { _key = ""; _value = ""; }
}

/// <summary>A game note entry item.</summary>
public partial class GameNoteItem : ObservableObject
{
    [ObservableProperty] private string _gameName;
    [ObservableProperty] private string _notes;
    [ObservableProperty] private string _notesUrl;
    [ObservableProperty] private string _notesUrlLabel;

    public GameNoteItem(string name, GameNoteEntry entry)
    {
        _gameName = name;
        _notes = entry.Notes ?? "";
        _notesUrl = entry.NotesUrl ?? "";
        _notesUrlLabel = entry.NotesUrlLabel ?? "";
    }
    public GameNoteItem() { _gameName = ""; _notes = ""; _notesUrl = ""; _notesUrlLabel = ""; }

    public GameNoteEntry ToEntry() => new()
    {
        Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
        NotesUrl = string.IsNullOrWhiteSpace(NotesUrl) ? null : NotesUrl,
        NotesUrlLabel = string.IsNullOrWhiteSpace(NotesUrlLabel) ? null : NotesUrlLabel,
    };
}
