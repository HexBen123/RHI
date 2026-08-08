using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class AuthorsPage : Page
{
    private AuthorsSectionViewModel? _vm;
    public AuthorsPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Authors == null) return;
        _vm = main.Authors;
        DonationView.ItemsSource = _vm.DonationUrls;
        AuthorOverridesView.ItemsSource = _vm.AuthorOverrides;
        NexusView.ItemsSource = _vm.NexusUrlOverrides;
        OptiScalerWikiView.ItemsSource = _vm.OptiScalerWikiNames;
    }

    private void AddDonation_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddDonationCommand.Execute(null);
    private void RemoveDonation_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveDonationCommand.Execute(i); }
    private void AddAuthorOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddAuthorOverrideCommand.Execute(null);
    private void RemoveAuthorOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveAuthorOverrideCommand.Execute(i); }
    private void AddNexus_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddNexusCommand.Execute(null);
    private void RemoveNexus_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveNexusCommand.Execute(i); }
    private void AddOptiScalerWiki_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddOptiScalerWikiCommand.Execute(null);
    private void RemoveOptiScalerWiki_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveOptiScalerWikiCommand.Execute(i); }
}
