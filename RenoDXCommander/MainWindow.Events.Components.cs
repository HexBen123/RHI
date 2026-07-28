// MainWindow.Events.Components.cs — Per-component cog button (⚙️) dialog handlers (RS, RDX, UL, DC, OS, DXVK).

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RenoDXCommander.Models;
using RenoDXCommander.Services;
using RenoDXCommander.ViewModels;

namespace RenoDXCommander;

public sealed partial class MainWindow
{
    // ── Component Cog Button Handlers ────────────────────────────────────────────

    private async void RsCogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: GameCardViewModel card }) return;
        if (string.IsNullOrEmpty(card.InstallPath)) return;

        var content = new StackPanel { Spacing = 8 };

        // Deploy ReShade.ini
        var deployIniBtn = new Button
        {
            Content = "Deploy ReShade.ini",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
        };
        deployIniBtn.Click += (s, ev) =>
        {
            try
            {
                var screenshotPath = BuildScreenshotSavePath(card.GameName);
                var overlayHotkey = ViewModel.Settings.OverlayHotkey;
                var screenshotHotkey = ViewModel.Settings.ScreenshotHotkey;
                if (card.RequiresVulkanInstall)
                {
                    AuxInstallService.MergeRsVulkanIni(card.InstallPath, card.GameName, screenshotPath, overlayHotkey, screenshotHotkey);
                    VulkanFootprintService.Create(card.InstallPath);
                    ViewModel.DeployShadersForCard(card.GameName);
                }
                else
                    AuxInstallService.MergeRsIni(card.InstallPath, screenshotPath, overlayHotkey, screenshotHotkey);

                if (card.UseUeExtended && card.Status == GameStatus.Installed)
                    AuxInstallService.ApplyRenoDxNativeHdrSettings(card.InstallPath);

                // Force-apply manifest [renodx] INI overrides on redeploy
                if (AuxInstallService.GlobalManifest?.RenodxIniOverrides != null
                    && AuxInstallService.GlobalManifest.RenodxIniOverrides.TryGetValue(card.GameName, out var cogIniOvr))
                    AuxInstallService.ApplyRenodxIniOverrides(card.InstallPath, cogIniOvr, forceOverwrite: true);

                card.RsActionMessage = "✅ ReShade.ini deployed.";
            }
            catch (Exception ex) { card.RsActionMessage = $"❌ {ex.Message}"; }
        };
        content.Children.Add(deployIniBtn);

        // Deploy ReShadePreset.ini
        var deployPresetBtn = new Button
        {
            Content = "Deploy ReShadePreset.ini",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = File.Exists(AuxInstallService.RsPresetIniPath),
        };
        deployPresetBtn.Click += (s, ev) =>
        {
            try
            {
                AuxInstallService.CopyRsPresetIniIfPresent(card.InstallPath);
                card.RsActionMessage = "✅ ReShadePreset.ini deployed.";
            }
            catch (Exception ex) { card.RsActionMessage = $"❌ {ex.Message}"; }
        };
        if (!File.Exists(AuxInstallService.RsPresetIniPath))
            ToolTipService.SetToolTip(deployPresetBtn, "No ReShadePreset.ini found in RHI config folder");
        content.Children.Add(deployPresetBtn);

        // Open ReShade.ini
        var openIniBtn = new Button
        {
            Content = "Open ReShade.ini",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderStrongBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = File.Exists(Path.Combine(card.InstallPath, "reshade.ini")),
        };
        openIniBtn.Click += async (s, ev) =>
        {
            var iniPath = Path.Combine(card.InstallPath, "reshade.ini");
            if (File.Exists(iniPath))
                await Windows.System.Launcher.LaunchUriAsync(new Uri(iniPath));
        };
        content.Children.Add(openIniBtn);

        // Open ReShade.log
        var openLogBtn = new Button
        {
            Content = "Open ReShade.log",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderStrongBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = File.Exists(Path.Combine(card.InstallPath, "ReShade.log")),
        };
        openLogBtn.Click += async (s, ev) =>
        {
            var logPath = Path.Combine(card.InstallPath, "ReShade.log");
            if (File.Exists(logPath))
                await Windows.System.Launcher.LaunchUriAsync(new Uri(logPath));
        };
        content.Children.Add(openLogBtn);

        // Copy ReShade.log to clipboard (as file, so Discord shows "ReShade.log")
        var copyLogBtn = new Button
        {
            Content = "Copy ReShade.log to clipboard",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderStrongBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = File.Exists(Path.Combine(card.InstallPath, "ReShade.log")),
        };
        copyLogBtn.Click += async (s, ev) =>
        {
            var logPath = Path.Combine(card.InstallPath, "ReShade.log");
            if (File.Exists(logPath))
            {
                try
                {
                    // Copy to temp as "ReShade.log" so clipboard file has the correct name
                    var tempDir = Path.Combine(Path.GetTempPath(), "RHI_clipboard");
                    Directory.CreateDirectory(tempDir);
                    var tempFile = Path.Combine(tempDir, "ReShade.log");
                    File.Copy(logPath, tempFile, overwrite: true);

                    var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFile);
                    var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dataPackage.SetStorageItems(new[] { storageFile });
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                    Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                    card.RsActionMessage = "✅ ReShade.log copied to clipboard.";
                    card.FadeMessage(m => card.RsActionMessage = m, card.RsActionMessage);
                }
                catch (Exception ex) { card.RsActionMessage = $"❌ {ex.Message}"; }
            }
        };
        content.Children.Add(copyLogBtn);

        var dialog = new ContentDialog
        {
            Title = "ReShade Settings",
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };
        await DialogService.ShowSafeAsync(dialog);
    }

    internal async void RdxCogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: GameCardViewModel card }) return;
        if (string.IsNullOrEmpty(card.InstallPath)) return;

        var iniPath = Path.Combine(card.InstallPath, "reshade.ini");
        var presetPath = Path.Combine(card.InstallPath, "RHI-RenoDX-Preset.txt");
        var content = new StackPanel { Spacing = 8 };
        bool hasRenoDxMod = !card.IsRtxHdrEnabled && (card.Mod?.SnapshotUrl != null || card.Status == GameStatus.Installed || card.Status == GameStatus.UpdateAvailable);

        // ── Top row: UE-Extended + Engine.ini HDR side by side ─────────────────
        if (card.UeExtendedToggleVisibility == Visibility.Visible || card.UseUeExtended)
        {
            content.Children.Add(new TextBlock
            {
                Text = "UE-Extended Settings",
                FontSize = 13,
                Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
            });
        }
        var topGrid = new Grid { ColumnSpacing = 12, RowSpacing = 6 };
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
        int topGridRow = 0;

        if (card.UeExtendedToggleVisibility == Visibility.Visible)
        {
            topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var ueLabel = new TextBlock { Text = "UE-Extended", FontSize = 11, Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(ueLabel, topGridRow);
            Grid.SetColumn(ueLabel, 0);
            topGrid.Children.Add(ueLabel);

            var ueCombo = new ComboBox { FontSize = 11, MinWidth = 100, HorizontalAlignment = HorizontalAlignment.Stretch };
            ueCombo.Items.Add("Off");
            ueCombo.Items.Add("On");
            ToolTipService.SetToolTip(ueCombo, "Switch between using UE-Extended or the game specific mod/generic Unreal RenoDX mod.");
            ueCombo.SelectedIndex = card.UseUeExtended ? 1 : 0;
            ueCombo.SelectionChanged += (s, ev) =>
            {
                bool enable = ueCombo.SelectedIndex == 1;
                if (enable != card.UseUeExtended)
                    ViewModel.ToggleUeExtended(card);
            };
            Grid.SetRow(ueCombo, topGridRow);
            Grid.SetColumn(ueCombo, 1);
            topGrid.Children.Add(ueCombo);
            topGridRow++;
        }

        // ── Peak Nits row (inside topGrid for alignment) ──────────────────────
        if (hasRenoDxMod && File.Exists(iniPath))
        {
            var peakIni = AuxInstallService.ParseIni(File.ReadAllLines(iniPath));
            var presetWithNits = peakIni.FirstOrDefault(kv =>
                kv.Key.StartsWith("renodx-preset", StringComparison.OrdinalIgnoreCase)
                && kv.Value.ContainsKey("ToneMapPeakNits"));
            string currentNits = "";
            if (presetWithNits.Value != null && presetWithNits.Value.TryGetValue("ToneMapPeakNits", out var nv))
                currentNits = double.TryParse(nv, out var dv) ? ((int)dv).ToString() : nv;

            topGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Label in column 0
            var nitsLabel = new TextBlock
            {
                Text = "Set Maximum Nits",
                FontSize = 11,
                Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(nitsLabel, topGridRow);
            Grid.SetColumn(nitsLabel, 0);
            topGrid.Children.Add(nitsLabel);

            var nitsBox = new TextBox
            {
                Text = currentNits,
                Width = 100,
                FontSize = 11,
                PlaceholderText = "nits",
                VerticalAlignment = VerticalAlignment.Center,
            };

            // Helper: write nits value to all preset sections
            void ApplyNitsValue(string nitsValue)
            {
                if (!int.TryParse(nitsValue, out var val) || val <= 0)
                {
                    card.ActionMessage = "❌ Enter a valid number.";
                    return;
                }
                try
                {
                    var freshIni = AuxInstallService.ParseIni(File.ReadAllLines(iniPath));
                    int updated = 0;
                    foreach (var section in freshIni)
                    {
                        if (section.Key.StartsWith("renodx-preset", StringComparison.OrdinalIgnoreCase))
                        {
                            section.Value["ToneMapPeakNits"] = val.ToString();
                            updated++;
                        }
                    }
                    if (updated == 0)
                    {
                        freshIni["renodx-preset1"] = new AuxInstallService.OrderedDict { ["ToneMapPeakNits"] = val.ToString() };
                        updated = 1;
                    }
                    AuxInstallService.WriteIni(iniPath, freshIni);
                    nitsBox.Text = val.ToString();
                    card.ActionMessage = $"✅ Set toneMapPeakNits={val} in {updated} preset(s).";
                    card.FadeMessage(m => card.ActionMessage = m, card.ActionMessage);
                }
                catch (Exception ex) { card.ActionMessage = $"❌ {ex.Message}"; }
            }

            // Enter key in TextBox applies the value and deselects
            nitsBox.KeyDown += (s, ev) =>
            {
                if (ev.Key == Windows.System.VirtualKey.Enter)
                {
                    ApplyNitsValue(nitsBox.Text);
                    nitsBox.IsEnabled = false;
                    nitsBox.IsEnabled = true;
                    ev.Handled = true;
                }
            };

            var autoBtn = new Button
            {
                Content = "Auto",
                Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
                Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
                BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8), Padding = new Thickness(10, 5, 10, 5), FontSize = 11,
            };
            ToolTipService.SetToolTip(autoBtn, "Reads your monitor's peak brightness automatically.");
            autoBtn.Click += async (s, ev) =>
            {
                try
                {
                    var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
                        Windows.Devices.Display.DisplayMonitor.GetDeviceSelector());
                    if (devices.Count == 0) { card.ActionMessage = "❌ No display found."; return; }

                    float maxNitsFound = 0;
                    foreach (var device in devices)
                    {
                        try
                        {
                            var mon = await Windows.Devices.Display.DisplayMonitor.FromInterfaceIdAsync(device.Id);
                            if (mon.MaxLuminanceInNits > maxNitsFound)
                                maxNitsFound = mon.MaxLuminanceInNits;
                        }
                        catch { }
                    }
                    var peakNits = (int)maxNitsFound;
                    if (peakNits <= 0) { card.ActionMessage = "❌ Could not read peak brightness."; return; }

                    ApplyNitsValue(peakNits.ToString());
                }
                catch (Exception ex) { card.ActionMessage = $"❌ {ex.Message}"; }
            };

            var nitsInputPanel = new StackPanel { Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
            nitsInputPanel.Children.Add(nitsBox);
            nitsInputPanel.Children.Add(autoBtn);
            Grid.SetRow(nitsInputPanel, topGridRow);
            Grid.SetColumn(nitsInputPanel, 1);
            Grid.SetColumnSpan(nitsInputPanel, 3);
            topGrid.Children.Add(nitsInputPanel);
            topGridRow++;
        }

        content.Children.Add(topGrid);

        // ── Compatibility Settings from [renodx] section ──────────────────────
        if (File.Exists(iniPath))
        {
            var ini = AuxInstallService.ParseIni(File.ReadAllLines(iniPath));
            if (ini.TryGetValue("renodx", out var renodxSection))
            {
                var upgradeKeys = renodxSection
                    .Where(kv => (kv.Key.StartsWith("Upgrade_", StringComparison.OrdinalIgnoreCase)
                                  && !kv.Key.Equals("Upgrade_UseSCRGB", StringComparison.OrdinalIgnoreCase)
                                  && !kv.Key.Equals("Upgrade_CopyDestinations", StringComparison.OrdinalIgnoreCase)
                                  && !kv.Key.Equals("Upgrade_SwapChainCompatibility", StringComparison.OrdinalIgnoreCase))
                              || kv.Key.Equals("Set_Path", StringComparison.OrdinalIgnoreCase)
                              || kv.Key.Equals("DumpLUTShaders", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(kv => kv.Key.Equals("DumpLUTShaders", StringComparison.OrdinalIgnoreCase) ? 1 : 0) // DumpLUT last
                    .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (upgradeKeys.Count > 0)
                {
                    content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 10, 0, 2) });
                    content.Children.Add(new TextBlock
                    {
                        Text = "Compatibility Settings",
                        FontSize = 13,
                        Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
                        Margin = new Thickness(0, 4, 0, 0),
                    });

                    var settingsGrid = new Grid { ColumnSpacing = 12, RowSpacing = 6 };
                    settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
                    settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    settingsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });

                    int totalRows = (upgradeKeys.Count + 1) / 2;
                    for (int r = 0; r < totalRows; r++)
                        settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    for (int i = 0; i < upgradeKeys.Count; i++)
                    {
                        var kv = upgradeKeys[i];
                        int row = i / 2;
                        int col = (i % 2) * 2; // 0 or 2

                        bool isSetPath = kv.Key.Equals("Set_Path", StringComparison.OrdinalIgnoreCase);
                        bool isDumpLut = kv.Key.Equals("DumpLUTShaders", StringComparison.OrdinalIgnoreCase);
                        bool isBinaryToggle = isSetPath || isDumpLut;

                        var label = new TextBlock
                        {
                            Text = isSetPath ? "Upgrade Path" : isDumpLut ? "Dump LUT Shaders" : kv.Key,
                            FontSize = 11,
                            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        Grid.SetRow(label, row);
                        Grid.SetColumn(label, col);
                        settingsGrid.Children.Add(label);

                        var combo = new ComboBox { FontSize = 11, MinWidth = 100, HorizontalAlignment = HorizontalAlignment.Stretch };

                        if (isSetPath) { combo.Items.Add("HDR"); combo.Items.Add("SDR"); }
                        else if (isDumpLut) { combo.Items.Add("Off"); combo.Items.Add("On"); }
                        else { combo.Items.Add("Off"); combo.Items.Add("Output size"); combo.Items.Add("Output ratio"); combo.Items.Add("Any size"); }

                        int.TryParse(kv.Value, out var currentVal);
                        combo.SelectedIndex = isBinaryToggle
                            ? (currentVal >= 0 && currentVal <= 1 ? currentVal : 0)
                            : (currentVal >= 0 && currentVal <= 3 ? currentVal : 0);

                        var capturedKey = kv.Key;
                        combo.SelectionChanged += (s, ev) =>
                        {
                            if (combo.SelectedIndex < 0) return;
                            renodxSection[capturedKey] = combo.SelectedIndex.ToString();
                            try { AuxInstallService.WriteIni(iniPath, ini); }
                            catch (Exception ex) { card.ActionMessage = $"❌ {ex.Message}"; }
                        };

                        Grid.SetRow(combo, row);
                        Grid.SetColumn(combo, col + 1);
                        settingsGrid.Children.Add(combo);
                    }

                    content.Children.Add(settingsGrid);

                    // ── Manifest-driven extra settings ──────────────────────────────────
                    var extraSettings = AuxInstallService.GlobalManifest?.RenodxExtraSettings;
                    if (extraSettings?.Count > 0)
                    {
                        // Append to the existing settings grid (continue from where hardcoded keys left off)
                        int startIdx = upgradeKeys.Count;
                        int extraRows = (startIdx + extraSettings.Count + 1) / 2 - settingsGrid.RowDefinitions.Count;
                        for (int r = 0; r < extraRows; r++)
                            settingsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        for (int i = 0; i < extraSettings.Count; i++)
                        {
                            var setting = extraSettings[i];
                            int idx = startIdx + i;
                            int row = idx / 2;
                            int col = (idx % 2) * 2;

                            var extraLabel = new TextBlock
                            {
                                Text = setting.Label ?? setting.Key,
                                FontSize = 11,
                                Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                                VerticalAlignment = VerticalAlignment.Center,
                            };
                            Grid.SetRow(extraLabel, row);
                            Grid.SetColumn(extraLabel, col);
                            settingsGrid.Children.Add(extraLabel);

                            var extraCombo = new ComboBox { FontSize = 11, MinWidth = 100, HorizontalAlignment = HorizontalAlignment.Stretch };

                            var options = setting.Options?.Count > 0
                                ? setting.Options
                                : new List<RenodxExtraOption> { new() { Value = "0", Name = "Off" }, new() { Value = "1", Name = "On" } };

                            foreach (var opt in options)
                                extraCombo.Items.Add(opt.Name);

                            string currentExtraVal = setting.Default;
                            if (renodxSection.TryGetValue(setting.Key, out var existingVal))
                                currentExtraVal = existingVal;
                            var selectedIdx = options.FindIndex(o => o.Value == currentExtraVal);
                            extraCombo.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

                            var capturedSetting = setting;
                            var capturedOptions = options;
                            extraCombo.SelectionChanged += (s, ev) =>
                            {
                                if (extraCombo.SelectedIndex < 0 || extraCombo.SelectedIndex >= capturedOptions.Count) return;
                                renodxSection[capturedSetting.Key] = capturedOptions[extraCombo.SelectedIndex].Value;
                                try { AuxInstallService.WriteIni(iniPath, ini); }
                                catch (Exception ex) { card.ActionMessage = $"❌ {ex.Message}"; }
                            };

                            Grid.SetRow(extraCombo, row);
                            Grid.SetColumn(extraCombo, col + 1);
                            settingsGrid.Children.Add(extraCombo);
                        }
                    }
                }
            }
            else
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Run the game once with RenoDX installed to generate settings.",
                    FontSize = 11,
                    Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                    FontStyle = Windows.UI.Text.FontStyle.Italic,
                    Margin = new Thickness(0, 4, 0, 0),
                });
            }
        }
        else
        {
            content.Children.Add(new TextBlock
            {
                Text = "No reshade.ini found in game folder.",
                FontSize = 11,
                Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                FontStyle = Windows.UI.Text.FontStyle.Italic,
            });
        }

        // ── Engine.ini Settings (only for Unreal Engine games) ────────────────
        if (card.EngineHint?.Contains("Unreal") == true && card.Status == GameStatus.Installed)
        {
            content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 10, 0, 2) });
            content.Children.Add(new TextBlock
            {
                Text = "Engine.ini Settings",
                FontSize = 13,
                Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
                Margin = new Thickness(0, 4, 0, 0),
            });

            var engineIniGrid = new Grid { ColumnSpacing = 12, RowSpacing = 6 };
            engineIniGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            engineIniGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            engineIniGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            engineIniGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            engineIniGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // HDR Settings toggle (only for UE-Extended games)
            if (card.UseUeExtended)
            {
                var hdrLabel = new TextBlock
                {
                    Text = "HDR Settings",
                    FontSize = 11,
                    Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(hdrLabel, 0);
                Grid.SetColumn(hdrLabel, 0);
                engineIniGrid.Children.Add(hdrLabel);

                var hdrCombo = new ComboBox { FontSize = 11, MinWidth = 100, HorizontalAlignment = HorizontalAlignment.Stretch };
                hdrCombo.Items.Add("Off");
                hdrCombo.Items.Add("On");
                ToolTipService.SetToolTip(hdrCombo, "Deploys Engine.ini with HDR flags for games that don't have an ingame HDR option. Disable for SDR.");
                bool hdrActive = card.InstalledRecord?.EngineIniHdr ?? true;
                hdrCombo.SelectedIndex = hdrActive ? 1 : 0;
                hdrCombo.SelectionChanged += (s, ev) =>
                {
                    if (hdrCombo.SelectedIndex == 1)
                    {
                        AuxInstallService.ApplyEngineIniHdrSettings(card.InstallPath, card.EngineIniProjectOverride, card.GameName);
                        if (card.InstalledRecord != null) card.InstalledRecord.EngineIniHdr = true;
                        card.ActionMessage = "✅ Engine.ini HDR settings deployed.";
                    }
                    else
                    {
                        AuxInstallService.RemoveEngineIniHdrSettings(card.InstallPath, card.EngineIniProjectOverride, card.GameName);
                        if (card.InstalledRecord != null) card.InstalledRecord.EngineIniHdr = false;
                        card.ActionMessage = "✅ Engine.ini HDR settings removed.";
                    }
                    if (card.InstalledRecord != null)
                        App.Services.GetRequiredService<IModInstallService>().SaveRecordPublic(card.InstalledRecord);
                    card.FadeMessage(m => card.ActionMessage = m, card.ActionMessage);
                };
                Grid.SetRow(hdrCombo, 0);
                Grid.SetColumn(hdrCombo, 1);
                engineIniGrid.Children.Add(hdrCombo);
            }

            // LUT Update Every Frame toggle
            int lutCol = card.UseUeExtended ? 2 : 0;
            var lutLabel = new TextBlock
            {
                Text = "LUT Update Every Frame",
                FontSize = 11,
                Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(lutLabel, 0);
            Grid.SetColumn(lutLabel, lutCol);
            engineIniGrid.Children.Add(lutLabel);

            var lutCombo = new ComboBox { FontSize = 11, MinWidth = 100, HorizontalAlignment = HorizontalAlignment.Stretch };
            lutCombo.Items.Add("Off");
            lutCombo.Items.Add("On");
            ToolTipService.SetToolTip(lutCombo, "Writes r.LUT.UpdateEveryFrame=1 to Engine.ini. Ensures the game recalculates LUTs each frame for accurate HDR color.");
            bool lutActive = card.InstalledRecord?.EngineIniLut ?? true;
            lutCombo.SelectedIndex = lutActive ? 1 : 0;
            lutCombo.SelectionChanged += (s, ev) =>
            {
                if (lutCombo.SelectedIndex == 1)
                {
                    AuxInstallService.ApplyEngineIniLutSetting(card.InstallPath, card.EngineIniProjectOverride, card.GameName);
                    if (card.InstalledRecord != null) card.InstalledRecord.EngineIniLut = true;
                    card.ActionMessage = "✅ LUT Update Every Frame enabled in Engine.ini.";
                }
                else
                {
                    AuxInstallService.RemoveEngineIniLutSetting(card.InstallPath, card.EngineIniProjectOverride, card.GameName);
                    if (card.InstalledRecord != null) card.InstalledRecord.EngineIniLut = false;
                    card.ActionMessage = "✅ LUT Update Every Frame removed from Engine.ini.";
                }
                if (card.InstalledRecord != null)
                    App.Services.GetRequiredService<IModInstallService>().SaveRecordPublic(card.InstalledRecord);
                card.FadeMessage(m => card.ActionMessage = m, card.ActionMessage);
            };
            Grid.SetRow(lutCombo, 0);
            Grid.SetColumn(lutCombo, lutCol + 1);
            engineIniGrid.Children.Add(lutCombo);

            content.Children.Add(engineIniGrid);
        }

        // ── Preset Export/Import buttons (side by side) ───────────────────────
        if (hasRenoDxMod)
        {
        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 10, 0, 2) });
        content.Children.Add(new TextBlock
        {
            Text = "RenoDX Presets",
            FontSize = 13,
            Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
            Margin = new Thickness(0, 8, 0, 0),
        });
        var presetRow = new StackPanel { Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal, Spacing = 8 };

        var exportBtn = new Button
        {
            Content = "Export Presets",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = File.Exists(iniPath),
        };
        exportBtn.Click += async (s, ev) =>
        {
            try
            {
                var lines = File.ReadAllLines(iniPath);
                var presetLines = new List<string>();
                bool inPreset = false;
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith("[renodx-preset", StringComparison.OrdinalIgnoreCase))
                    {
                        inPreset = true;
                        if (presetLines.Count > 0) presetLines.Add("");
                        presetLines.Add(line);
                    }
                    else if (line.TrimStart().StartsWith('[') && inPreset)
                    {
                        inPreset = false;
                    }
                    else if (inPreset)
                    {
                        presetLines.Add(line);
                    }
                }

                if (presetLines.Count == 0)
                {
                    card.ActionMessage = "❌ No [renodx-preset*] sections found.";
                    return;
                }

                // Add header comment
                presetLines.Insert(0, $"; RenoDX Preset exported from: {card.GameName}");
                presetLines.Insert(1, "; To import: place this file in the game folder and click 'Import Presets' in RHI,");
                presetLines.Insert(2, "; or paste the [renodx-preset*] sections into reshade.ini manually.");
                presetLines.Insert(3, "");

                File.WriteAllLines(presetPath, presetLines);
                // Copy as file to clipboard (shows as RHI-RenoDX-Preset.txt in Discord)
                try
                {
                    var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(presetPath);
                    var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dp.SetStorageItems(new[] { storageFile });
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
                    Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                }
                catch { /* clipboard copy is best-effort */ }
                card.ActionMessage = $"✅ Exported {presetLines.Count(l => l.StartsWith("["))} preset(s) & copied to clipboard.";
                card.FadeMessage(m => card.ActionMessage = m, card.ActionMessage);
            }
            catch (Exception ex) { card.ActionMessage = $"❌ {ex.Message}"; }
        };
        ToolTipService.SetToolTip(exportBtn, "Save all RenoDX presets to a file and copy to clipboard for sharing.");
        presetRow.Children.Add(exportBtn);

        var importBtn = new Button
        {
            Content = "Import Presets",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = File.Exists(presetPath) && File.Exists(iniPath),
        };
        importBtn.Click += (s, ev) =>
        {
            try
            {
                // Read preset file, skip comment lines (header)
                var presetLines = File.ReadAllLines(presetPath)
                    .Where(l => !l.TrimStart().StartsWith(';'))
                    .ToArray();
                var iniLines = File.ReadAllLines(iniPath).ToList();

                // Collect preset section names from the backup file
                var presetSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in presetLines)
                {
                    if (line.TrimStart().StartsWith("[renodx-preset", StringComparison.OrdinalIgnoreCase))
                        presetSections.Add(line.Trim());
                }

                // Remove existing preset sections from reshade.ini
                var filtered = new List<string>();
                bool skipping = false;
                foreach (var line in iniLines)
                {
                    if (line.TrimStart().StartsWith("[renodx-preset", StringComparison.OrdinalIgnoreCase))
                    {
                        skipping = true;
                        continue;
                    }
                    if (line.TrimStart().StartsWith('[') && skipping)
                        skipping = false;
                    if (!skipping)
                        filtered.Add(line);
                }

                // Append imported presets at the end
                filtered.Add("");
                filtered.AddRange(presetLines);

                File.WriteAllLines(iniPath, filtered);
                card.ActionMessage = $"✅ Imported {presetSections.Count} preset(s).";
                card.FadeMessage(m => card.ActionMessage = m, card.ActionMessage);
            }
            catch (Exception ex) { card.ActionMessage = $"❌ {ex.Message}"; }
        };
        if (!File.Exists(presetPath))
            ToolTipService.SetToolTip(importBtn, "No RHI-RenoDX-Preset.txt file found. Export first.");
        else
            ToolTipService.SetToolTip(importBtn, "Restore presets from the exported backup file into reshade.ini.");
        presetRow.Children.Add(importBtn);
        content.Children.Add(presetRow);
        } // end hasRenoDxMod

        // ── RTX HDR Toggle ─────────────────────────────────────────────────────
        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 10, 0, 2) });
        content.Children.Add(new TextBlock
        {
            Text = "RTX HDR",
            FontSize = 13,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
        });

        var rtxHdrCombo = new ComboBox { FontSize = 11, MinWidth = 100 };
        rtxHdrCombo.Items.Add("Off");
        rtxHdrCombo.Items.Add("On");

        var gameNameService = App.Services.GetRequiredService<IGameNameService>();
        bool isRtxHdrEnabled = gameNameService.RtxHdrGames.Contains(card.GameName);
        rtxHdrCombo.SelectedIndex = isRtxHdrEnabled ? 1 : 0;

        rtxHdrCombo.SelectionChanged += async (s, ev) =>
        {
            bool enable = rtxHdrCombo.SelectedIndex == 1;
            var dlssPresetService = App.Services.GetRequiredService<DlssPresetService>();

            if (enable)
            {
                gameNameService.RtxHdrGames.Add(card.GameName);
                card.IsRtxHdrEnabled = true;

                // Uninstall RenoDX if installed
                if (card.Status == GameStatus.Installed && card.InstalledRecord != null)
                {
                    ViewModel.UninstallMod(card);
                }

                // Set RTX HDR profile settings (Allow + Enable + sensible defaults)
                dlssPresetService.SetRtxHdrAllow(card.GameName, card.InstallPath, 0x01);
                dlssPresetService.SetRtxHdrEnable(card.GameName, card.InstallPath, 0x01);
                dlssPresetService.SetRtxHdrPeakBrightness(card.GameName, card.InstallPath, (uint)(ViewModel.Settings.PeakNits > 0 ? ViewModel.Settings.PeakNits : 510));
                dlssPresetService.SetRtxHdrContrast(card.GameName, card.InstallPath, 100);       // 0 (neutral)
                dlssPresetService.SetRtxHdrSaturation(card.GameName, card.InstallPath, 100);     // 0 (neutral)
                dlssPresetService.SetRtxHdrMiddleGrey(card.GameName, card.InstallPath, 50);      // default

                CrashReporter.Log($"[RdxCogButton_Click] RTX HDR enabled for '{card.GameName}'");
            }
            else
            {
                gameNameService.RtxHdrGames.Remove(card.GameName);
                card.IsRtxHdrEnabled = false;

                // Delete all RTX HDR settings from profile (revert to global/inherited)
                // Some settings (0x00DD48Fx) can't be deleted via NvAPI — write defaults instead
                dlssPresetService.DeleteSettingRaw(card.GameName, card.InstallPath, 0x1077A11A); // Allow (deletable)
                dlssPresetService.SetRtxHdrEnable(card.GameName, card.InstallPath, 0x00);        // Enable → Off
                dlssPresetService.SetRtxHdrContrast(card.GameName, card.InstallPath, 100);       // Contrast → 0 (default)
                dlssPresetService.SetRtxHdrSaturation(card.GameName, card.InstallPath, 100);     // Saturation → 0 (default)
                dlssPresetService.SetRtxHdrPeakBrightness(card.GameName, card.InstallPath, 0);   // Peak Brightness → N/A
                dlssPresetService.SetRtxHdrMiddleGrey(card.GameName, card.InstallPath, 50);      // Middle Grey → default
                dlssPresetService.DeleteSettingRaw(card.GameName, card.InstallPath, 0x00432F84); // Debanding (deletable)

                CrashReporter.Log($"[RdxCogButton_Click] RTX HDR disabled for '{card.GameName}' — all settings deleted from profile");
            }

            card.NotifyAll();
            ViewModel.SaveSettingsPublic();
            _detailPanelBuilder?.UpdateDetailComponentRows(card);
        };

        var rtxHdrRow = new StackPanel { Orientation = Microsoft.UI.Xaml.Controls.Orientation.Horizontal, Spacing = 12 };
        rtxHdrRow.Children.Add(new TextBlock
        {
            Text = "Enable RTX HDR",
            FontSize = 11,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        });
        rtxHdrRow.Children.Add(rtxHdrCombo);
        content.Children.Add(rtxHdrRow);

        var dialog = new ContentDialog
        {
            Title = "RenoDX Settings",
            Content = new ScrollViewer { Content = content, MaxHeight = 620, Padding = new Thickness(0, 0, 16, 0) },
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };
        dialog.Resources["ContentDialogMaxWidth"] = 800.0;
        await DialogService.ShowSafeAsync(dialog);
        _detailPanelBuilder?.UpdateDetailComponentRows(card);
    }

    internal async void RtxHdrConfigButton_Click(object sender, RoutedEventArgs e)
    {
        var card = (sender as FrameworkElement)?.Tag as GameCardViewModel
                ?? (sender as Button)?.Tag as GameCardViewModel;
        if (card == null || string.IsNullOrEmpty(card.InstallPath)) return;

        var dlssPresetService = App.Services.GetRequiredService<DlssPresetService>();
        var content = new StackPanel { Spacing = 12 };

        // Read current values
        var currentContrast = (int)dlssPresetService.GetRtxHdrContrast(card.GameName, card.InstallPath);
        var currentSaturation = (int)dlssPresetService.GetRtxHdrSaturation(card.GameName, card.InstallPath);
        var currentPeakBrightness = (int)dlssPresetService.GetRtxHdrPeakBrightness(card.GameName, card.InstallPath);
        var currentMiddleGrey = (int)dlssPresetService.GetRtxHdrMiddleGrey(card.GameName, card.InstallPath);
        var currentDebanding = (int)dlssPresetService.GetRtxHdrDebanding(card.GameName, card.InstallPath);

        // Convert stored values to display values
        int contrastDisplay = currentContrast > 0 ? currentContrast - 100 : 0;
        int saturationDisplay = currentSaturation > 0 ? currentSaturation - 100 : 0;
        int peakBrightnessDisplay = currentPeakBrightness > 0 ? currentPeakBrightness : ViewModel.Settings.PeakNits;
        if (peakBrightnessDisplay < 400) peakBrightnessDisplay = 510; // fallback default

        // ── Peak Brightness ───────────────────────────────────────────────────
        var nitsLabel = new TextBlock { Text = $"Peak Brightness: {peakBrightnessDisplay} nits", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) };
        var nitsSlider = new Slider { Minimum = 400, Maximum = 2000, StepFrequency = 10, Value = peakBrightnessDisplay, HorizontalAlignment = HorizontalAlignment.Stretch };
        nitsSlider.ValueChanged += (s, ev) => nitsLabel.Text = $"Peak Brightness: {(int)nitsSlider.Value} nits";
        content.Children.Add(nitsLabel);
        content.Children.Add(nitsSlider);

        // ── Contrast ──────────────────────────────────────────────────────────
        string ContrastLabel(int val) => val switch
        {
            0 => "Contrast: 0 — Gamma 2.0 (Default)",
            25 => "Contrast: +25 — Gamma 2.2",
            50 => "Contrast: +50 — Gamma 2.4",
            _ => $"Contrast: {(val >= 0 ? "+" : "")}{val}",
        };
        var contrastLabel = new TextBlock { Text = ContrastLabel(contrastDisplay), FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) };
        var contrastSlider = new Slider { Minimum = -100, Maximum = 100, StepFrequency = 1, Value = contrastDisplay, HorizontalAlignment = HorizontalAlignment.Stretch };
        contrastSlider.ValueChanged += (s, ev) => contrastLabel.Text = ContrastLabel((int)contrastSlider.Value);
        content.Children.Add(contrastLabel);
        content.Children.Add(contrastSlider);

        // ── Saturation ────────────────────────────────────────────────────────
        var satLabel = new TextBlock { Text = $"Saturation: {saturationDisplay}", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) };
        var satSlider = new Slider { Minimum = -100, Maximum = 100, StepFrequency = 1, Value = saturationDisplay, HorizontalAlignment = HorizontalAlignment.Stretch };
        satSlider.ValueChanged += (s, ev) => satLabel.Text = $"Saturation: {(int)satSlider.Value}";
        content.Children.Add(satLabel);
        content.Children.Add(satSlider);

        // ── Middle Grey ───────────────────────────────────────────────────────
        var middleGreyValues = new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 };
        var middleGreyCombo = new ComboBox { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Stretch };
        int selectedMgIndex = 8; // default = 50
        for (int i = 0; i < middleGreyValues.Length; i++)
        {
            var val = middleGreyValues[i];
            middleGreyCombo.Items.Add(val == 50 ? "50 (Default)" : val.ToString());
            if (currentMiddleGrey == val) selectedMgIndex = i;
        }
        middleGreyCombo.SelectedIndex = selectedMgIndex;
        var mgLabel = new TextBlock { Text = "Middle Grey", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) };
        content.Children.Add(mgLabel);
        content.Children.Add(middleGreyCombo);

        // ── Debanding ─────────────────────────────────────────────────────────
        var debandingOptions = new (string name, uint value)[]
        {
            ("No Debanding", 0x06),
            ("Low Debanding", 0x0A),
            ("High Debanding", 0x02),
            ("High Debanding (Indicator)", 0x03),
            ("High Debanding (Indicator + Debug)", 0x23),
        };
        var debandingCombo = new ComboBox { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Stretch };
        int selectedDbIndex = 0;
        for (int i = 0; i < debandingOptions.Length; i++)
        {
            debandingCombo.Items.Add(debandingOptions[i].name);
            if (currentDebanding == (int)debandingOptions[i].value) selectedDbIndex = i;
        }
        debandingCombo.SelectedIndex = selectedDbIndex;
        var dbLabel = new TextBlock { Text = "Debanding", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) };
        content.Children.Add(dbLabel);
        content.Children.Add(debandingCombo);

        // ── Dialog ────────────────────────────────────────────────────────────
        var dialog = new ContentDialog
        {
            Title = "RTX HDR Settings",
            Content = new ScrollViewer { Content = content, MaxHeight = 520, Padding = new Thickness(0, 0, 16, 0) },
            PrimaryButtonText = "Apply",
            CloseButtonText = "Cancel",
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };

        var result = await DialogService.ShowSafeAsync(dialog);
        if (result != ContentDialogResult.Primary) return;

        // Write all values
        var peakNits = (uint)nitsSlider.Value;
        var contrastStored = (uint)(100 + (int)contrastSlider.Value);
        var satStored = (uint)(100 + (int)satSlider.Value);
        var middleGrey = (uint)middleGreyValues[middleGreyCombo.SelectedIndex];
        var debanding = debandingOptions[debandingCombo.SelectedIndex].value;

        dlssPresetService.SetRtxHdrPeakBrightness(card.GameName, card.InstallPath, peakNits);
        dlssPresetService.SetRtxHdrContrast(card.GameName, card.InstallPath, contrastStored);
        dlssPresetService.SetRtxHdrSaturation(card.GameName, card.InstallPath, satStored);
        dlssPresetService.SetRtxHdrMiddleGrey(card.GameName, card.InstallPath, middleGrey);
        dlssPresetService.SetRtxHdrDebanding(card.GameName, card.InstallPath, debanding);

        CrashReporter.Log($"[RtxHdrConfigButton_Click] Applied RTX HDR settings for '{card.GameName}': PeakNits={peakNits}, Contrast={contrastStored}, Sat={satStored}, MidGrey={middleGrey}, Deband=0x{debanding:X2}");
        card.ActionMessage = "✅ RTX HDR settings applied.";
        card.FadeMessage(m => card.ActionMessage = m, card.ActionMessage);
    }

    private async void UlCogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: GameCardViewModel card }) return;
        var content = new StackPanel { Spacing = 8 };
        var deployBtn = new Button
        {
            Content = "Deploy relimiter.ini",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
        };
        deployBtn.Click += (s, ev) =>
        {
            if (string.IsNullOrEmpty(card.InstallPath)) return;
            try
            {
                AuxInstallService.CopyUlIni(card.InstallPath);
                card.UlActionMessage = "✅ relimiter.ini copied to game folder.";
            }
            catch (Exception ex) { card.UlActionMessage = $"❌ {ex.Message}"; }
        };
        content.Children.Add(deployBtn);

        // Find the relimiter log file (relimiter_*.log)
        string? logFile = null;
        if (!string.IsNullOrEmpty(card.InstallPath) && Directory.Exists(card.InstallPath))
        {
            try
            {
                logFile = Directory.GetFiles(card.InstallPath, "relimiter_*.log").FirstOrDefault();
            }
            catch { /* ignore access errors */ }
        }

        var logName = logFile != null ? Path.GetFileName(logFile) : "relimiter_*.log";

        // Open relimiter log
        var openLogBtn = new Button
        {
            Content = "Open ReLimiter log",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderStrongBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = logFile != null,
        };
        openLogBtn.Click += async (s, ev) =>
        {
            if (logFile != null && File.Exists(logFile))
                await Windows.System.Launcher.LaunchUriAsync(new Uri(logFile));
        };
        content.Children.Add(openLogBtn);

        // Copy relimiter log to clipboard (as file with correct name)
        var copyLogBtn = new Button
        {
            Content = "Copy ReLimiter log to clipboard",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.SurfaceOverlayBrush),
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.BorderStrongBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
            IsEnabled = logFile != null,
        };
        copyLogBtn.Click += async (s, ev) =>
        {
            if (logFile != null && File.Exists(logFile))
            {
                try
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), "RHI_clipboard");
                    Directory.CreateDirectory(tempDir);
                    var tempFile = Path.Combine(tempDir, Path.GetFileName(logFile));
                    File.Copy(logFile, tempFile, overwrite: true);

                    var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFile);
                    var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dataPackage.SetStorageItems(new[] { storageFile });
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                    Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                    card.UlActionMessage = $"✅ {Path.GetFileName(logFile)} copied to clipboard.";
                    card.FadeMessage(m => card.UlActionMessage = m, card.UlActionMessage);
                }
                catch (Exception ex) { card.UlActionMessage = $"❌ {ex.Message}"; }
            }
        };
        content.Children.Add(copyLogBtn);

        // ── Target FPS Setting ────────────────────────────────────────────────
        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 10, 0, 2) });
        content.Children.Add(new TextBlock
        {
            Text = "Frame Limiter",
            FontSize = 13,
            Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
            Margin = new Thickness(0, 4, 0, 0),
        });

        // Target FPS per-game control
        var targetFpsPanel = new Grid { ColumnSpacing = 12 };
        targetFpsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        targetFpsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var targetFpsLabel = new TextBlock
        {
            Text = "Target FPS",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(targetFpsLabel, 0);
        targetFpsPanel.Children.Add(targetFpsLabel);
        var targetFpsCombo = new ComboBox { FontSize = 12, MinWidth = 140, HorizontalAlignment = HorizontalAlignment.Right };
        ToolTipService.SetToolTip(targetFpsCombo, "FPS cap for this game. Select a VRR preset or Custom for a manual value.");
        Grid.SetColumn(targetFpsCombo, 1);
        targetFpsPanel.Children.Add(targetFpsCombo);

        // VRR preset options (same as global settings)
        var vrrPresets = new (int Fps, string Label)[]
        {
            (59,  "59 (60Hz VRR)"),
            (73,  "73 (75Hz VRR)"),
            (97,  "97 (100Hz VRR)"),
            (116, "116 (120Hz VRR)"),
            (138, "138 (144Hz VRR)"),
            (157, "157 (165Hz VRR)"),
            (171, "171 (180Hz VRR)"),
            (189, "189 (200Hz VRR)"),
            (224, "224 (240Hz VRR)"),
            (258, "258 (280Hz VRR)"),
            (275, "275 (300Hz VRR)"),
            (324, "324 (360Hz VRR)"),
            (416, "416 (480Hz VRR)"),
            (431, "431 (500Hz VRR)"),
        };
        var vrrFpsSet = new HashSet<int>(vrrPresets.Select(p => p.Fps));

        // Read current per-game value from the game's relimiter.ini
        int currentTargetFps = 0;
        if (!string.IsNullOrEmpty(card.InstallPath))
        {
            var ulIniFile = Path.Combine(card.InstallPath, "relimiter.ini");
            if (File.Exists(ulIniFile))
            {
                try
                {
                    var ulIni = AuxInstallService.ParseIni(File.ReadAllLines(ulIniFile));
                    if (ulIni.TryGetValue("FrameLimiter", out var flSection)
                        && flSection.TryGetValue("target_fps", out var fpsVal)
                        && int.TryParse(fpsVal, out var parsedFps))
                    {
                        currentTargetFps = parsedFps;
                    }
                }
                catch { /* use default 0 = off */ }
            }
        }

        // Inline custom FPS input (shown when "Custom..." is selected)
        var customFpsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Visibility = Visibility.Collapsed };
        var customFpsBox = new TextBox { PlaceholderText = "20-1000", FontSize = 12, MinWidth = 100 };
        var customFpsBtn = new Button { Content = "Set", FontSize = 12 };
        customFpsPanel.Children.Add(customFpsBox);
        customFpsPanel.Children.Add(customFpsBtn);

        // Populate combo
        bool suppressFpsChange = true;
        targetFpsCombo.Items.Add("Off");
        foreach (var preset in vrrPresets)
            targetFpsCombo.Items.Add(preset.Label);

        // If current value is a custom FPS (not in presets), insert it before "Custom..."
        if (currentTargetFps > 0 && !vrrFpsSet.Contains(currentTargetFps))
            targetFpsCombo.Items.Add($"{currentTargetFps} (Custom)");

        targetFpsCombo.Items.Add("Custom...");

        // Select based on current value
        if (currentTargetFps == 0)
            targetFpsCombo.SelectedIndex = 0; // Off
        else
        {
            int matchIdx = Array.FindIndex(vrrPresets, p => p.Fps == currentTargetFps);
            if (matchIdx >= 0)
                targetFpsCombo.SelectedIndex = matchIdx + 1; // +1 for "Off" at index 0
            else
            {
                // Custom value — select the "(Custom)" item
                targetFpsCombo.SelectedIndex = targetFpsCombo.Items.Count - 2; // before "Custom..."
            }
        }
        suppressFpsChange = false;

        // Helper to refresh combo after setting custom value
        void RefreshFpsCombo(int newFps)
        {
            suppressFpsChange = true;
            currentTargetFps = newFps;
            targetFpsCombo.Items.Clear();
            targetFpsCombo.Items.Add("Off");
            foreach (var preset in vrrPresets)
                targetFpsCombo.Items.Add(preset.Label);
            if (newFps > 0 && !vrrFpsSet.Contains(newFps))
                targetFpsCombo.Items.Add($"{newFps} (Custom)");
            targetFpsCombo.Items.Add("Custom...");

            if (newFps == 0)
                targetFpsCombo.SelectedIndex = 0;
            else
            {
                int idx = Array.FindIndex(vrrPresets, p => p.Fps == newFps);
                if (idx >= 0)
                    targetFpsCombo.SelectedIndex = idx + 1;
                else
                    targetFpsCombo.SelectedIndex = targetFpsCombo.Items.Count - 2; // Custom item
            }
            customFpsPanel.Visibility = Visibility.Collapsed;
            suppressFpsChange = false;
        }

        targetFpsCombo.SelectionChanged += (s, ev) =>
        {
            if (suppressFpsChange) return;
            if (string.IsNullOrEmpty(card.InstallPath)) return;

            var selectedText = targetFpsCombo.SelectedItem as string ?? "";

            // "Custom..." shows inline TextBox for manual entry
            if (selectedText == "Custom...")
            {
                customFpsPanel.Visibility = Visibility.Visible;
                customFpsBox.Text = "";
                customFpsBox.Focus(FocusState.Programmatic);
                return;
            }

            customFpsPanel.Visibility = Visibility.Collapsed;

            // Handle preset/Off selection
            int newFps;
            var idx = targetFpsCombo.SelectedIndex;
            if (idx == 0)
                newFps = 0; // Off
            else if (idx - 1 < vrrPresets.Length)
                newFps = vrrPresets[idx - 1].Fps;
            else
                return; // Custom label item — don't set

            var iniFile = Path.Combine(card.InstallPath, "relimiter.ini");
            if (File.Exists(iniFile))
            {
                try
                {
                    AuxInstallService.ApplyUlTargetFps(iniFile, newFps);
                    currentTargetFps = newFps;
                    card.UlActionMessage = newFps == 0
                        ? "✅ Target FPS disabled for this game."
                        : $"✅ Target FPS set to {newFps} for this game.";
                    card.FadeMessage(m => card.UlActionMessage = m, card.UlActionMessage);
                }
                catch (Exception ex) { card.UlActionMessage = $"❌ {ex.Message}"; }
            }
        };

        // Custom FPS "Set" button handler
        customFpsBtn.Click += (s, ev) =>
        {
            if (string.IsNullOrEmpty(card.InstallPath)) return;
            if (int.TryParse(customFpsBox.Text, out var customFps) && customFps >= 20 && customFps <= 1000)
            {
                var iniFile = Path.Combine(card.InstallPath, "relimiter.ini");
                if (File.Exists(iniFile))
                {
                    try
                    {
                        AuxInstallService.ApplyUlTargetFps(iniFile, customFps);
                        card.UlActionMessage = $"✅ Target FPS set to {customFps} for this game.";
                        card.FadeMessage(m => card.UlActionMessage = m, card.UlActionMessage);
                        RefreshFpsCombo(customFps);
                    }
                    catch (Exception ex) { card.UlActionMessage = $"❌ {ex.Message}"; }
                }
            }
        };

        // Enter key also sets custom value
        customFpsBox.KeyDown += (s, ev) =>
        {
            if (ev.Key == Windows.System.VirtualKey.Enter)
            {
                if (string.IsNullOrEmpty(card.InstallPath)) return;
                if (int.TryParse(customFpsBox.Text, out var customFps) && customFps >= 20 && customFps <= 1000)
                {
                    var iniFile = Path.Combine(card.InstallPath, "relimiter.ini");
                    if (File.Exists(iniFile))
                    {
                        try
                        {
                            AuxInstallService.ApplyUlTargetFps(iniFile, customFps);
                            card.UlActionMessage = $"✅ Target FPS set to {customFps} for this game.";
                            card.FadeMessage(m => card.UlActionMessage = m, card.UlActionMessage);
                            RefreshFpsCombo(customFps);
                        }
                        catch (Exception ex) { card.UlActionMessage = $"❌ {ex.Message}"; }
                    }
                }
            }
        };

        content.Children.Add(targetFpsPanel);
        content.Children.Add(customFpsPanel);

        // ── Compatibility Settings ────────────────────────────────────────────
        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 10, 0, 2) });
        content.Children.Add(new TextBlock
        {
            Text = "Compatibility Settings",
            FontSize = 13,
            Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush),
            Margin = new Thickness(0, 4, 0, 0),
        });

        // DLSS Hooks per-game toggle
        var dlssHooksPanel = new Grid { ColumnSpacing = 12 };
        dlssHooksPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        dlssHooksPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var dlssHooksLabel = new TextBlock
        {
            Text = "DLSS Hooks",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(dlssHooksLabel, 0);
        dlssHooksPanel.Children.Add(dlssHooksLabel);
        var dlssHooksCombo = new ComboBox { FontSize = 12, MinWidth = 80, HorizontalAlignment = HorizontalAlignment.Right };
        dlssHooksCombo.Items.Add("Off");
        dlssHooksCombo.Items.Add("On");
        ToolTipService.SetToolTip(dlssHooksCombo, "Shows DLSS version/preset info on the ReLimiter OSD. Disable if causing crashes.");
        Grid.SetColumn(dlssHooksCombo, 1);
        dlssHooksPanel.Children.Add(dlssHooksCombo);

        // Read current per-game value from the game's relimiter.ini
        bool currentDlssHooks = ViewModel.Settings.UlDlssHooks; // default to global
        if (!string.IsNullOrEmpty(card.InstallPath))
        {
            var ulIniFile = Path.Combine(card.InstallPath, "relimiter.ini");
            if (File.Exists(ulIniFile))
            {
                try
                {
                    var ulIni = AuxInstallService.ParseIni(File.ReadAllLines(ulIniFile));
                    if (ulIni.TryGetValue("FrameLimiter", out var flSection)
                        && flSection.TryGetValue("dlss_info_hooks", out var hooksVal))
                    {
                        currentDlssHooks = hooksVal.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch { /* use global default */ }
            }
        }
        dlssHooksCombo.SelectedIndex = currentDlssHooks ? 1 : 0;
        dlssHooksCombo.SelectionChanged += (s, ev) =>
        {
            if (string.IsNullOrEmpty(card.InstallPath)) return;
            var ulIniFile = Path.Combine(card.InstallPath, "relimiter.ini");
            if (File.Exists(ulIniFile))
            {
                try
                {
                    AuxInstallService.ApplyUlDlssHooks(ulIniFile, dlssHooksCombo.SelectedIndex == 1);
                    card.UlActionMessage = dlssHooksCombo.SelectedIndex == 1
                        ? "✅ DLSS Hooks enabled for this game."
                        : "✅ DLSS Hooks disabled for this game.";
                    card.FadeMessage(m => card.UlActionMessage = m, card.UlActionMessage);
                }
                catch (Exception ex) { card.UlActionMessage = $"❌ {ex.Message}"; }
            }
        };
        content.Children.Add(dlssHooksPanel);

        var dialog = new ContentDialog
        {
            Title = "ReLimiter Settings",
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };
        await DialogService.ShowSafeAsync(dialog);
    }

    private async void DcCogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: GameCardViewModel card }) return;
        var content = new StackPanel { Spacing = 12 };
        var deployBtn = new Button
        {
            Content = "Deploy DisplayCommander.ini",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
        };
        deployBtn.Click += (s, ev) =>
        {
            if (string.IsNullOrEmpty(card.InstallPath)) return;
            try
            {
                AuxInstallService.CopyDcIni(card.InstallPath);
                card.DcActionMessage = "✅ DisplayCommander.ini copied to game folder.";
                card.FadeMessage(m => card.DcActionMessage = m, card.DcActionMessage);
            }
            catch (Exception ex) { card.DcActionMessage = $"❌ {ex.Message}"; }
        };
        content.Children.Add(deployBtn);

        var dialog = new ContentDialog
        {
            Title = "Display Commander Settings",
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };
        await DialogService.ShowSafeAsync(dialog);
    }

    private async void OsCogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: GameCardViewModel card }) return;
        var content = new StackPanel { Spacing = 12 };
        var deployBtn = new Button
        {
            Content = "Deploy OptiScaler.ini",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
        };
        deployBtn.Click += (s, ev) => _installEventHandler.CopyOsIniButton_Click(sender, e);
        content.Children.Add(deployBtn);

        var dialog = new ContentDialog
        {
            Title = "OptiScaler Settings",
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };
        await DialogService.ShowSafeAsync(dialog);
    }

    private async void DxvkCogButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: GameCardViewModel card }) return;
        var content = new StackPanel { Spacing = 12 };
        var deployBtn = new Button
        {
            Content = "Deploy dxvk.conf",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = UIFactory.Brush(ResourceKeys.AccentBlueBgBrush),
            Foreground = UIFactory.Brush(ResourceKeys.AccentBlueBrush),
            BorderBrush = UIFactory.Brush(ResourceKeys.AccentBlueBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 7, 12, 7), FontSize = 12,
        };
        deployBtn.Click += (s, ev) => ViewModel.CopyDxvkConf(card);
        content.Children.Add(deployBtn);

        var dialog = new ContentDialog
        {
            Title = "DXVK Settings",
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };
        await DialogService.ShowSafeAsync(dialog);
    }

    private void SupportGuide_Click(object sender, RoutedEventArgs e)
    {
        _ = Windows.System.Launcher.LaunchUriAsync(
            new Uri("https://github.com/RankFTW/RHI/blob/main/docs/DETAILED_GUIDE.md"));
    }

    private void SupportKofi_Click(object sender, RoutedEventArgs e)
    {
        _ = Windows.System.Launcher.LaunchUriAsync(
            new Uri("https://ko-fi.com/rankftw"));
    }

}