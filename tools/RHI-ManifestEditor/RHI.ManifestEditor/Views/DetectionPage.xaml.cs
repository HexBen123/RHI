using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class DetectionPage : Page
{
    private DetectionSectionViewModel? _vm;

    public DetectionPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Detection == null) return;
        _vm = main.Detection;
        BlacklistView.ItemsSource = _vm.Blacklist;
        WikiUnlinksView.ItemsSource = _vm.WikiUnlinks;
        WikiNameOverridesView.ItemsSource = _vm.WikiNameOverrides;
        InstallPathOverridesView.ItemsSource = _vm.InstallPathOverrides;
    }

    private void AddBlacklist_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddBlacklistCommand.Execute(null);
    private void RemoveBlacklist_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Microsoft.UI.Xaml.Controls.Button)s).Tag is ViewModels.StringItem item) _vm?.RemoveBlacklistCommand.Execute(item); }

    private void AddWikiUnlink_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddWikiUnlinkCommand.Execute(null);
    private void RemoveWikiUnlink_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Microsoft.UI.Xaml.Controls.Button)s).Tag is ViewModels.StringItem item) _vm?.RemoveWikiUnlinkCommand.Execute(item); }

    private void AddWikiNameOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddWikiNameOverrideCommand.Execute(null);
    private void RemoveWikiNameOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Microsoft.UI.Xaml.Controls.Button)s).Tag is ViewModels.KeyValueItem item) _vm?.RemoveWikiNameOverrideCommand.Execute(item); }

    private void AddInstallPathOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddInstallPathOverrideCommand.Execute(null);
    private void RemoveInstallPathOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Microsoft.UI.Xaml.Controls.Button)s).Tag is ViewModels.KeyValueItem item) _vm?.RemoveInstallPathOverrideCommand.Execute(item); }
}
