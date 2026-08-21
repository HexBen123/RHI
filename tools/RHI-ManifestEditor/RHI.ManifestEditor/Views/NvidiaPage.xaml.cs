using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class NvidiaPage : Page
{
    private NvidiaSectionViewModel? _vm;
    private bool _urlInitializing;

    public NvidiaPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Nvidia == null) return;
        _vm = main.Nvidia;

        _urlInitializing = true;
        RtxHdrInfoUrlBox.Text = _vm.RtxHdrInfoUrl;
        _urlInitializing = false;

        ProfileNamesView.ItemsSource = _vm.ProfileNameOverrides;
        ExeExclusionsView.ItemsSource = _vm.ProfileExeExclusions;
        SrPresetsView.ItemsSource = _vm.DlssSrPresets;
        RrPresetsView.ItemsSource = _vm.DlssRrPresets;
        FgPresetsView.ItemsSource = _vm.DlssFgPresets;
        SrPresetsDevView.ItemsSource = _vm.DlssSrPresetsDev;
        RrPresetsDevView.ItemsSource = _vm.DlssRrPresetsDev;
        FgPresetsDevView.ItemsSource = _vm.DlssFgPresetsDev;
    }

    private void RtxHdrInfoUrl_TextChanged(object s, TextChangedEventArgs e)
    {
        if (_urlInitializing || _vm == null) return;
        _vm.RtxHdrInfoUrl = RtxHdrInfoUrlBox.Text;
    }

    private void AddProfileName_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddProfileOverrideCommand.Execute(null);
    private void RemoveProfileName_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveProfileOverrideCommand.Execute(i); }

    private void AddExeExclusion_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddExeExclusionCommand.Execute(null);
    private void RemoveExeExclusion_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem i) _vm?.RemoveExeExclusionCommand.Execute(i); }

    private void AddSrPreset_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddSrPresetCommand.Execute(null);
    private void RemoveSrPreset_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is DlssPresetItem i) _vm?.RemoveSrPresetCommand.Execute(i); }
    private void AddRrPreset_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddRrPresetCommand.Execute(null);
    private void RemoveRrPreset_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is DlssPresetItem i) _vm?.RemoveRrPresetCommand.Execute(i); }
    private void AddFgPreset_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddFgPresetCommand.Execute(null);
    private void RemoveFgPreset_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is DlssPresetItem i) _vm?.RemoveFgPresetCommand.Execute(i); }

    private void AddSrPresetDev_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddSrPresetDevCommand.Execute(null);
    private void RemoveSrPresetDev_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is DlssPresetItem i) _vm?.RemoveSrPresetDevCommand.Execute(i); }
    private void AddRrPresetDev_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddRrPresetDevCommand.Execute(null);
    private void RemoveRrPresetDev_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is DlssPresetItem i) _vm?.RemoveRrPresetDevCommand.Execute(i); }
    private void AddFgPresetDev_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddFgPresetDevCommand.Execute(null);
    private void RemoveFgPresetDev_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is DlssPresetItem i) _vm?.RemoveFgPresetDevCommand.Execute(i); }
}
