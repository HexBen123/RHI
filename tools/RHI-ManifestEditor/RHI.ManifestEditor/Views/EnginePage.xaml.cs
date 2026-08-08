using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class EnginePage : Page
{
    private EngineSectionViewModel? _vm;
    public EnginePage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not MainViewModel main || main.Engine == null) return;
        _vm = main.Engine;
        EngineOverridesView.ItemsSource = _vm.EngineOverrides;
        EngineHintView.ItemsSource = _vm.EngineHintOverrides;
        GraphicsApiView.ItemsSource = _vm.GraphicsApiOverrides;
        Bit32View.ItemsSource = _vm.ThirtyTwoBitGames;
        Bit64View.ItemsSource = _vm.SixtyFourBitGames;
    }

    private void AddEngineOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddEngineOverrideCommand.Execute(null);
    private void RemoveEngineOverride_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveEngineOverrideCommand.Execute(i); }
    private void AddEngineHint_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddEngineHintOverrideCommand.Execute(null);
    private void RemoveEngineHint_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveEngineHintOverrideCommand.Execute(i); }
    private void AddGraphicsApi_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.AddGraphicsApiOverrideCommand.Execute(null);
    private void RemoveGraphicsApi_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is KeyValueItem i) _vm?.RemoveGraphicsApiOverrideCommand.Execute(i); }
    private void Add32Bit_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.Add32BitCommand.Execute(null);
    private void Remove32Bit_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem i) _vm?.Remove32BitCommand.Execute(i); }
    private void Add64Bit_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e) => _vm?.Add64BitCommand.Execute(null);
    private void Remove64Bit_Click(object s, Microsoft.UI.Xaml.RoutedEventArgs e)
    { if (((Button)s).Tag is StringItem i) _vm?.Remove64BitCommand.Execute(i); }
}
