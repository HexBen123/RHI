using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class DxvkPage : Page
{
    private DxvkSectionViewModel? _vm;
    public DxvkPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Dxvk == null) return;
        _vm = main.Dxvk;
        BlacklistView.ItemsSource = _vm.DxvkBlacklist;
        NotesView.ItemsSource = _vm.DxvkGameNotes;
    }

    private void AddBlacklist_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddDxvkBlacklistCommand.Execute(null);
    private void RemoveBlacklist_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem i) _vm?.RemoveDxvkBlacklistCommand.Execute(i); }
    private void AddNote_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddDxvkNoteCommand.Execute(null);
    private void RemoveNote_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is GameNoteItem i) _vm?.RemoveDxvkNoteCommand.Execute(i); }
}
