using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RHI.ManifestEditor.ViewModels;
using RHI.ManifestEditor.Views;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace RHI.ManifestEditor;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        TitleText.Text = ViewModel.Title;
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.Title))
                DispatcherQueue.TryEnqueue(() => TitleText.Text = ViewModel.Title);
            if (e.PropertyName == nameof(ViewModel.StatusMessage))
                DispatcherQueue.TryEnqueue(() => StatusText.Text = ViewModel.StatusMessage ?? "");
            if (e.PropertyName == nameof(ViewModel.ValidationError))
                DispatcherQueue.TryEnqueue(() =>
                {
                    ValidationText.Text = ViewModel.ValidationError ?? "";
                    ValidationText.Visibility = string.IsNullOrEmpty(ViewModel.ValidationError)
                        ? Visibility.Collapsed : Visibility.Visible;
                });
        };

        // Auto-open if default manifest exists
        var defaultPath = @"G:\RDXC\manifest.json";
        if (File.Exists(defaultPath))
            ViewModel.OpenFileCommand.Execute(defaultPath);

        // Default to search page
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private async void OpenBtn_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        picker.FileTypeFilter.Add(".json");
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        var file = await picker.PickSingleFileAsync();
        if (file != null)
            ViewModel.OpenFileCommand.Execute(file.Path);
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
        => ViewModel.SaveCommand.Execute(null);

    private async void SaveAsBtn_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker();
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        picker.FileTypeChoices.Add("JSON", new List<string> { ".json" });
        picker.SuggestedFileName = "manifest.json";
        var file = await picker.PickSaveFileAsync();
        if (file != null)
            ViewModel.SaveAsCommand.Execute(file.Path);
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item) return;
        var tag = item.Tag?.ToString();
        Type? pageType = tag switch
        {
            "search"     => typeof(GameSearchPage),
            "detection"  => typeof(DetectionPage),
            "engine"     => typeof(EnginePage),
            "uehdr"      => typeof(UeHdrPage),
            "install"    => typeof(InstallPage),
            "notes"      => typeof(NotesPage),
            "dxvk"       => typeof(DxvkPage),
            "wikistatus" => typeof(WikiStatusPage),
            "authors"    => typeof(AuthorsPage),
            "nvidia"     => typeof(NvidiaPage),
            "doffix"     => typeof(DofFixPage),
            "packs"      => typeof(PacksPage),
            _            => null
        };
        if (pageType != null)
            ContentFrame.Navigate(pageType, ViewModel);
    }
}
