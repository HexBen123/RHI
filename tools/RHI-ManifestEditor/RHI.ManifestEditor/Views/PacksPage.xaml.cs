using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class PacksPage : Page
{
    private PacksSectionViewModel? _vm;
    public PacksPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Packs == null) return;
        _vm = main.Packs;
        ComponentUrlsView.ItemsSource = _vm.ComponentUrls;
    }

    private void AddComponentUrl_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddComponentUrlCommand.Execute(null);
    private void RemoveComponentUrl_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveComponentUrlCommand.Execute(i); }
}
