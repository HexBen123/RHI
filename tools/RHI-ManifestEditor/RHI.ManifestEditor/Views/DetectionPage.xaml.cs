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
        BlacklistPrefixesView.ItemsSource = _vm.BlacklistPrefixes;
        WikiUnlinksView.ItemsSource = _vm.WikiUnlinks;
        WikiNameOverridesView.ItemsSource = _vm.WikiNameOverrides;
        LumaNameOverridesView.ItemsSource = _vm.LumaNameOverrides;
        InstallPathOverridesView.ItemsSource = _vm.InstallPathOverrides;
        SplitGamesView.ItemsSource = _vm.SplitGames;
        SteamAppIdView.ItemsSource = _vm.SteamAppIdOverrides;
        EmulatorGamesView.ItemsSource = _vm.EmulatorGames;
    }

    private void AddBlacklist_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddBlacklistCommand.Execute(null);
    private void RemoveBlacklist_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem item) _vm?.RemoveBlacklistCommand.Execute(item); }

    private void AddBlacklistPrefix_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddBlacklistPrefixCommand.Execute(null);
    private void RemoveBlacklistPrefix_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem item) _vm?.RemoveBlacklistPrefixCommand.Execute(item); }

    private void AddWikiUnlink_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddWikiUnlinkCommand.Execute(null);
    private void RemoveWikiUnlink_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem item) _vm?.RemoveWikiUnlinkCommand.Execute(item); }

    private void AddWikiNameOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddWikiNameOverrideCommand.Execute(null);
    private void RemoveWikiNameOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem item) _vm?.RemoveWikiNameOverrideCommand.Execute(item); }

    private void AddLumaNameOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddLumaNameOverrideCommand.Execute(null);
    private void RemoveLumaNameOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem item) _vm?.RemoveLumaNameOverrideCommand.Execute(item); }

    private void AddInstallPathOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddInstallPathOverrideCommand.Execute(null);
    private void RemoveInstallPathOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem item) _vm?.RemoveInstallPathOverrideCommand.Execute(item); }

    private void AddSplitGame_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddSplitGameCommand.Execute(null);
    private void RemoveSplitGame_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is SplitGameItem item) _vm?.RemoveSplitGameCommand.Execute(item); }
    private void AddSubGame_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is SplitGameItem item) item.AddSubGameCommand.Execute(null); }
    private void RemoveSubGame_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (((Button)s).Tag is not SplitGameEntryItem entry) return;
        if (_vm == null) return;
        foreach (var sg in _vm.SplitGames)
            if (sg.SubGames.Contains(entry)) { sg.RemoveSubGameCommand.Execute(entry); break; }
    }

    private void AddSteamAppId_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddSteamAppIdCommand.Execute(null);
    private void RemoveSteamAppId_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem item) _vm?.RemoveSteamAppIdCommand.Execute(item); }

    private void AddEmulatorGame_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddEmulatorGameCommand.Execute(null);
    private void RemoveEmulatorGame_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is EmulatorGameItem item) _vm?.RemoveEmulatorGameCommand.Execute(item); }

    private void AddEmulatorAddon_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is EmulatorGameItem item) item.AddAddonCommand.Execute(null); }
    private void RemoveEmulatorAddon_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (((Button)s).Tag is not StringItem entry) return;
        if (_vm == null) return;
        foreach (var eg in _vm.EmulatorGames)
            if (eg.Addons.Contains(entry)) { eg.RemoveAddonCommand.Execute(entry); break; }
    }
    private void AddEmulatorAddonUrl_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is EmulatorGameItem item) item.AddAddonUrlCommand.Execute(null); }
    private void RemoveEmulatorAddonUrl_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (((Button)s).Tag is not KeyValueItem entry) return;
        if (_vm == null) return;
        foreach (var eg in _vm.EmulatorGames)
            if (eg.AddonUrls.Contains(entry)) { eg.RemoveAddonUrlCommand.Execute(entry); break; }
    }
}
