using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class InstallPage : Page
{
    private InstallSectionViewModel? _vm;
    public InstallPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Install == null) return;
        _vm = main.Install;
        InstallWarningsView.ItemsSource = _vm.InstallWarnings;
        SnapshotView.ItemsSource = _vm.SnapshotOverrides;
        DllOverrideView.ItemsSource = _vm.DllNameOverrides;
        LaunchExeView.ItemsSource = _vm.LaunchExeOverrides;
        LegacyVersionsView.ItemsSource = _vm.LegacyReShadeVersions;
    }

    private void AddInstallWarning_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddInstallWarningCommand.Execute(null);
    private void RemoveInstallWarning_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is InstallWarningItem i) _vm?.RemoveInstallWarningCommand.Execute(i); }
    private void AddSnapshot_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddSnapshotOverrideCommand.Execute(null);
    private void RemoveSnapshot_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveSnapshotOverrideCommand.Execute(i); }
    private void AddDllOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddDllOverrideCommand.Execute(null);
    private void RemoveDllOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is DllOverrideItem i) _vm?.RemoveDllOverrideCommand.Execute(i); }
    private void AddLaunchExe_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddLaunchExeOverrideCommand.Execute(null);
    private void RemoveLaunchExe_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveLaunchExeOverrideCommand.Execute(i); }
    private void AddLegacyVersion_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddLegacyVersionCommand.Execute(null);
    private void RemoveLegacyVersion_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveLegacyVersionCommand.Execute(i); }
}
