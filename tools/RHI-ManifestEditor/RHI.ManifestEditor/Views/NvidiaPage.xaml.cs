using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class NvidiaPage : Page
{
    private NvidiaSectionViewModel? _vm;
    public NvidiaPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Nvidia == null) return;
        _vm = main.Nvidia;
        ProfileNamesView.ItemsSource = _vm.ProfileNameOverrides;
        ExeExclusionsView.ItemsSource = _vm.ProfileExeExclusions;
    }

    private void AddProfileName_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddProfileOverrideCommand.Execute(null);
    private void RemoveProfileName_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveProfileOverrideCommand.Execute(i); }
    private void AddExeExclusion_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddExeExclusionCommand.Execute(null);
    private void RemoveExeExclusion_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem i) _vm?.RemoveExeExclusionCommand.Execute(i); }
}
