using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class WikiStatusPage : Page
{
    private WikiStatusSectionViewModel? _vm;
    public WikiStatusPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.WikiStatus == null) return;
        _vm = main.WikiStatus;
        ListView.ItemsSource = _vm.WikiStatusOverrides;
    }

    private void Add_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddCommand.Execute(null);
    private void Remove_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveCommand.Execute(i); }
}
