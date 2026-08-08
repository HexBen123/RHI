using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RHI.ManifestEditor.ViewModels;

namespace RHI.ManifestEditor.Views;

public sealed partial class GameSearchPage : Page
{
    private MainViewModel? _vm;

    public GameSearchPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _vm = e.Parameter as MainViewModel;
        if (_vm != null)
            NameList.ItemsSource = _vm.SearchResults;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_vm != null) _vm.SearchQuery = SearchBox.Text;
    }

    private void NameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        EntryPanel.Children.Clear();
        if (_vm == null || NameList.SelectedItem is not GameSearchResult result) return;
        var entries = _vm.GetEntriesForGame(result.GameName);

        EntryPanel.Children.Add(new TextBlock
        {
            Text = result.GameName,
            FontSize = 16,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 8)
        });

        if (entries.Count == 0)
        {
            EntryPanel.Children.Add(new TextBlock
            {
                Text = "No entries found.",
                Foreground = (Microsoft.UI.Xaml.Media.SolidColorBrush)App.Current.Resources["TextFillColorSecondaryBrush"]
            });
            return;
        }

        foreach (var (field, section, value) in entries)
        {
            var card = new Border
            {
                Padding = new Microsoft.UI.Xaml.Thickness(12, 8, 12, 8),
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(6),
                Background = (Microsoft.UI.Xaml.Media.Brush)App.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)App.Current.Resources["CardStrokeColorDefaultBrush"],
            };
            var sp = new StackPanel { Spacing = 2 };
            sp.Children.Add(new TextBlock
            {
                Text = $"{section} › {field}",
                FontSize = 11,
                Foreground = (Microsoft.UI.Xaml.Media.Brush)App.Current.Resources["AccentTextFillColorPrimaryBrush"]
            });
            sp.Children.Add(new TextBlock
            {
                Text = value.Length > 120 ? value[..120] + "…" : value,
                FontSize = 12,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            });
            card.Child = sp;
            EntryPanel.Children.Add(card);
        }
    }
}
