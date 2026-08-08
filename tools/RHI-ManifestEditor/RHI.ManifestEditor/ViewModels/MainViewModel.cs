using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RHI.ManifestEditor.Models;
using RHI.ManifestEditor.Services;

namespace RHI.ManifestEditor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ManifestFileService _svc = new();

    [ObservableProperty] private string _title = "RHI Manifest Editor";
    [ObservableProperty] private string? _filePath;
    [ObservableProperty] private bool _hasUnsavedChanges;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private string? _validationError;

    // The live manifest model
    public RemoteManifest? Manifest { get; private set; }

    // Raw JsonObject for comment key preservation
    private JsonObject? _rawJson;

    // Section ViewModels
    public DetectionSectionViewModel? Detection { get; private set; }
    public EngineSectionViewModel? Engine { get; private set; }
    public UeHdrSectionViewModel? UeHdr { get; private set; }
    public InstallSectionViewModel? Install { get; private set; }
    public NotesSectionViewModel? Notes { get; private set; }
    public DxvkSectionViewModel? Dxvk { get; private set; }
    public WikiStatusSectionViewModel? WikiStatus { get; private set; }
    public AuthorsSectionViewModel? Authors { get; private set; }
    public NvidiaSectionViewModel? Nvidia { get; private set; }
    public DofFixSectionViewModel? DofFix { get; private set; }
    public PacksSectionViewModel? Packs { get; private set; }

    // Game search
    [ObservableProperty] private string _searchQuery = "";
    public ObservableCollection<GameSearchResult> SearchResults { get; } = new();

    // Section navigation
    [ObservableProperty] private int _selectedSectionIndex = 0;

    partial void OnSearchQueryChanged(string value) => RunSearch(value);
    partial void OnHasUnsavedChangesChanged(bool value) => UpdateTitle();
    partial void OnFilePathChanged(string? value) => UpdateTitle();

    private void UpdateTitle()
    {
        var name = FilePath != null ? Path.GetFileName(FilePath) : "No file";
        Title = $"RHI Manifest Editor — {name}{(HasUnsavedChanges ? " *" : "")}";
    }

    [RelayCommand]
    public void OpenFile(string path)
    {
        try
        {
            var (manifest, raw) = _svc.Load(path);
            Manifest = manifest;
            _rawJson = raw;
            FilePath = path;
            HasUnsavedChanges = false;
            BuildSectionViewModels();
            StatusMessage = $"Loaded {Path.GetFileName(path)} — v{manifest.Version}";
            ValidationError = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading: {ex.Message}";
        }
    }

    [RelayCommand]
    public void Save()
    {
        if (FilePath == null || Manifest == null || _rawJson == null) return;
        try
        {
            CommitSectionChanges();
            _svc.Save(FilePath, Manifest, _rawJson);
            HasUnsavedChanges = false;
            StatusMessage = "Saved.";

            // Validate after save
            var err = _svc.Validate(FilePath);
            ValidationError = err;
            if (err != null) StatusMessage = "Saved but validation failed: " + err;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save error: {ex.Message}";
        }
    }

    [RelayCommand]
    public void SaveAs(string newPath)
    {
        FilePath = newPath;
        Save();
    }

    public void MarkDirty() => HasUnsavedChanges = true;

    private void BuildSectionViewModels()
    {
        if (Manifest == null) return;
        Detection = new DetectionSectionViewModel(Manifest, this);
        Engine    = new EngineSectionViewModel(Manifest, this);
        UeHdr     = new UeHdrSectionViewModel(Manifest, this);
        Install   = new InstallSectionViewModel(Manifest, this);
        Notes     = new NotesSectionViewModel(Manifest, this);
        Dxvk      = new DxvkSectionViewModel(Manifest, this);
        WikiStatus = new WikiStatusSectionViewModel(Manifest, this);
        Authors   = new AuthorsSectionViewModel(Manifest, this);
        Nvidia    = new NvidiaSectionViewModel(Manifest, this);
        DofFix    = new DofFixSectionViewModel(Manifest, this);
        Packs     = new PacksSectionViewModel(Manifest, this);
        OnPropertyChanged(nameof(Detection)); OnPropertyChanged(nameof(Engine));
        OnPropertyChanged(nameof(UeHdr));     OnPropertyChanged(nameof(Install));
        OnPropertyChanged(nameof(Notes));     OnPropertyChanged(nameof(Dxvk));
        OnPropertyChanged(nameof(WikiStatus));OnPropertyChanged(nameof(Authors));
        OnPropertyChanged(nameof(Nvidia));    OnPropertyChanged(nameof(DofFix));
        OnPropertyChanged(nameof(Packs));
    }

    private void CommitSectionChanges()
    {
        if (Manifest == null) return;
        Detection?.Commit(); Engine?.Commit(); UeHdr?.Commit();
        Install?.Commit();   Notes?.Commit();  Dxvk?.Commit();
        WikiStatus?.Commit();Authors?.Commit();Nvidia?.Commit();
        DofFix?.Commit();    Packs?.Commit();
    }

    private void RunSearch(string query)
    {
        SearchResults.Clear();
        if (Manifest == null || string.IsNullOrWhiteSpace(query) || query.Length < 2) return;
        var allNames = ManifestFileService.GetAllGameNames(Manifest)
            .Where(n => n.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(50);
        foreach (var name in allNames)
            SearchResults.Add(new GameSearchResult { GameName = name });
    }

    public List<(string Field, string Section, string Value)> GetEntriesForGame(string gameName)
    {
        if (Manifest == null) return new();
        CommitSectionChanges();
        return ManifestFileService.GetEntriesForGame(Manifest, gameName);
    }
}

public class GameSearchResult
{
    public string GameName { get; set; } = "";
}
