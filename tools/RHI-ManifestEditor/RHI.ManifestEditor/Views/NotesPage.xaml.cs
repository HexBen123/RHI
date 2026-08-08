using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class NotesPage : Page
{
    private NotesSectionViewModel? _vm;
    private string _currentTab = "gameNotes";

    public NotesPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Notes == null) return;
        _vm = main.Notes;
        LoadTab(_currentTab);
        NotesTabView.SelectedIndex = 0;
    }

    private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NotesTabView.SelectedItem is TabViewItem item)
        {
            _currentTab = item.Tag?.ToString() ?? "gameNotes";
            LoadTab(_currentTab);
        }
    }

    private void LoadTab(string tab)
    {
        if (_vm == null) return;
        NotesList.ItemsSource = tab switch
        {
            "gameNotes"              => _vm.GameNotes,
            "reshadeGameInfo"        => _vm.ReshadeGameInfo,
            "lumaGameInfo"           => _vm.LumaGameInfo,
            "lumaGameNotes"          => _vm.LumaGameNotes,
            "reframeworkGameInfo"    => _vm.ReframeworkGameInfo,
            "relimiterGameInfo"      => _vm.RelimiterGameInfo,
            "displayCommanderGameInfo" => _vm.DisplayCommanderGameInfo,
            "optiScalerGameInfo"     => _vm.OptiScalerGameInfo,
            _                        => _vm.GameNotes
        };
    }

    private void AddNote_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_vm == null) return;
        var col = GetCurrentCol();
        col?.Add(new GameNoteItem());
        _vm._main.MarkDirty();  // Will be accessed via public property
    }

    private void RemoveNote_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_vm == null) return;
        if (((Button)s).Tag is GameNoteItem item)
        {
            GetCurrentCol()?.Remove(item);
            _vm._main.MarkDirty();
        }
    }

    private ObservableCollection<GameNoteItem>? GetCurrentCol() => _currentTab switch
    {
        "gameNotes"              => _vm?.GameNotes,
        "reshadeGameInfo"        => _vm?.ReshadeGameInfo,
        "lumaGameInfo"           => _vm?.LumaGameInfo,
        "lumaGameNotes"          => _vm?.LumaGameNotes,
        "reframeworkGameInfo"    => _vm?.ReframeworkGameInfo,
        "relimiterGameInfo"      => _vm?.RelimiterGameInfo,
        "displayCommanderGameInfo" => _vm?.DisplayCommanderGameInfo,
        "optiScalerGameInfo"     => _vm?.OptiScalerGameInfo,
        _                        => _vm?.GameNotes
    };
}
