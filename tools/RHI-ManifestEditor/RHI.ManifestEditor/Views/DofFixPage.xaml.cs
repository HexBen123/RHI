using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class DofFixPage : Page
{
    private DofFixSectionViewModel? _vm;
    public DofFixPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.DofFix == null) return;
        _vm = main.DofFix;
        ForceView.ItemsSource = _vm.DofFixForceGames;
        SkipView.ItemsSource = _vm.DofFixSkipGames;
    }

    private void AddForce_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddForceCommand.Execute(null);
    private void RemoveForce_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem i) _vm?.RemoveForceCommand.Execute(i); }
    private void AddSkip_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddSkipCommand.Execute(null);
    private void RemoveSkip_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem i) _vm?.RemoveSkipCommand.Execute(i); }
}
