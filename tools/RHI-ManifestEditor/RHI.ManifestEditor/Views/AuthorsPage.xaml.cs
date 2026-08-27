using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class AuthorsPage : Page
{
    private AuthorsSectionViewModel? _vm;
    private bool _pcgwInitializing;

    public AuthorsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Authors == null) return;
        _vm = main.Authors;

        _pcgwInitializing = true;
        PcgwUseAppIdToggle.IsOn = _vm.PcgwUseAppId;
        PcgwUrlCacheVersionBox.Text = _vm.PcgwUrlCacheVersion;
        _pcgwInitializing = false;

        AuthorDisplayNamesView.ItemsSource = _vm.AuthorDisplayNames;
        DonationView.ItemsSource = _vm.DonationUrls;
        AuthorOverridesView.ItemsSource = _vm.AuthorOverrides;
        NexusView.ItemsSource = _vm.NexusUrlOverrides;
        PcgwView.ItemsSource = _vm.PcgwUrlOverrides;
        UwFixView.ItemsSource = _vm.UwFixUrlOverrides;
        UltraPlusView.ItemsSource = _vm.UltraPlusUrlOverrides;
        OptiScalerWikiView.ItemsSource = _vm.OptiScalerWikiNames;
    }

    private void PcgwUseAppId_Toggled(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_pcgwInitializing || _vm == null) return;
        _vm.PcgwUseAppId = PcgwUseAppIdToggle.IsOn;
    }

    private void PcgwUrlCacheVersion_TextChanged(object s, TextChangedEventArgs e)
    {
        if (_pcgwInitializing || _vm == null) return;
        _vm.PcgwUrlCacheVersion = PcgwUrlCacheVersionBox.Text;
    }

    private void AddAuthorDisplayName_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddAuthorDisplayCommand.Execute(null);
    private void RemoveAuthorDisplayName_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveAuthorDisplayCommand.Execute(i); }

    private void AddDonation_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddDonationCommand.Execute(null);
    private void RemoveDonation_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveDonationCommand.Execute(i); }
    private void AddAuthorOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddAuthorOverrideCommand.Execute(null);
    private void RemoveAuthorOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveAuthorOverrideCommand.Execute(i); }
    private void AddNexus_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddNexusCommand.Execute(null);
    private void RemoveNexus_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveNexusCommand.Execute(i); }
    private void AddPcgw_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) { _vm?.PcgwUrlOverrides.Add(new KeyValueItem()); _vm?._main.MarkDirty(); }
    private void RemovePcgw_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) { _vm?.PcgwUrlOverrides.Remove(i); _vm?._main.MarkDirty(); } }
    private void AddUwFix_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) { _vm?.UwFixUrlOverrides.Add(new KeyValueItem()); _vm?._main.MarkDirty(); }
    private void RemoveUwFix_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) { _vm?.UwFixUrlOverrides.Remove(i); _vm?._main.MarkDirty(); } }
    private void AddUltraPlus_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) { _vm?.UltraPlusUrlOverrides.Add(new KeyValueItem()); _vm?._main.MarkDirty(); }
    private void RemoveUltraPlus_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) { _vm?.UltraPlusUrlOverrides.Remove(i); _vm?._main.MarkDirty(); } }
    private void AddOptiScalerWiki_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddOptiScalerWikiCommand.Execute(null);
    private void RemoveOptiScalerWiki_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveOptiScalerWikiCommand.Execute(i); }
}
