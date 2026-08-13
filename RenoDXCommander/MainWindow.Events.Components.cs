// MainWindow.Events.Components.cs — Per-component cog button (⚙️) dialog handlers (RS, RDX, UL, DC, OS, DXVK).

using System;
using System.IO;
using System.Text.Json;
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

        // ── Overlay Key ───────────────────────────────────────────────────────
        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 2, 0, 2) });
        content.Children.Add(new TextBlock
        {
            Text = "Overlay Key",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            Margin = new Thickness(0, 4, 0, 0),
        });

        // Read current key from reshade.ini (game folder)
        var iniPath = Path.Combine(card.InstallPath, "reshade.ini");
        string currentHotkey = ViewModel.Settings.OverlayHotkey; // fallback to global
        if (File.Exists(iniPath))
        {
            try
            {
                var ini = AuxInstallService.ParseIni(File.ReadAllLines(iniPath));
                if (ini.TryGetValue("INPUT", out var inputSection)
                    && inputSection.TryGetValue("KeyOverlay", out var ko)
                    && !string.IsNullOrWhiteSpace(ko))
                    currentHotkey = ko;
            }
            catch { /* use fallback */ }
        }

        var hotkeyString = currentHotkey;
        var hotkeyBox = new TextBox
        {
            Text = HotkeyManager.FormatHotkeyDisplay(hotkeyString),
            IsReadOnly = true,
            PlaceholderText = "Click then press a key...",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        ToolTipService.SetToolTip(hotkeyBox, "Click here then press your desired key. Written to all reshade*.ini files for this game.");

        hotkeyBox.GotFocus += (s, ev) => hotkeyBox.Text = "Press a key...";
        hotkeyBox.KeyDown += (s, ev) =>
        {
            var vk = (int)ev.Key;
            if (vk == 0 || vk == 16 || vk == 17 || vk == 18) return; // ignore modifiers alone
            bool shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            bool ctrl  = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            bool alt   = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            hotkeyString = HotkeyManager.BuildHotkeyString(vk, shift, ctrl, alt);
            hotkeyBox.Text = HotkeyManager.FormatHotkeyDisplay(hotkeyString);
            ev.Handled = true;
        };
        hotkeyBox.LostFocus += (s, ev) =>
        {
            if (hotkeyBox.Text == "Press a key...")
                hotkeyBox.Text = HotkeyManager.FormatHotkeyDisplay(hotkeyString);
        };

        var applyKeyBtn = new Button
        {
            Content = "Apply",
            FontSize = 12,
            Padding = new Thickness(16, 7, 16, 7),
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        applyKeyBtn.Click += (s, ev) =>
        {
            if (string.IsNullOrEmpty(card.InstallPath)) return;
            try
            {
                // Write to all reshade*.ini files in the game folder
                var iniFiles = Directory.EnumerateFiles(card.InstallPath, "reshade*.ini")
                    .Where(f => Path.GetExtension(f).Equals(".ini", StringComparison.OrdinalIgnoreCase)
                             && Path.GetFileNameWithoutExtension(f).StartsWith("reshade", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var file in iniFiles)
                    AuxInstallService.ApplyOverlayHotkey(file, hotkeyString);
                applyKeyBtn.Content = "Applied!";
                _crashReporter.Log($"[RsCogButton_Click] Applied overlay key '{hotkeyString}' to {iniFiles.Count} ini file(s) for '{card.GameName}'");
            }
            catch (Exception ex) { card.RsActionMessage = $"❌ {ex.Message}"; }
        };

        var keyGrid = new Grid { ColumnSpacing = 8 };
        keyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        keyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        keyGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        keyGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        hotkeyBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        Grid.SetColumn(hotkeyBox, 0); Grid.SetRow(hotkeyBox, 0);
        Grid.SetColumn(applyKeyBtn, 1); Grid.SetRow(applyKeyBtn, 0);
        keyGrid.Children.Add(hotkeyBox);
        keyGrid.Children.Add(applyKeyBtn);
        content.Children.Add(keyGrid);

        // ── Screenshot Key ────────────────────────────────────────────────────
        content.Children.Add(new TextBlock
        {
            Text = "Screenshot Key",
            FontSize = 12,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            Margin = new Thickness(0, 6, 0, 0),
        });

        string currentScreenshotHotkey = ViewModel.Settings.ScreenshotHotkey;
        if (File.Exists(iniPath))
        {
            try
            {
                var ini2 = AuxInstallService.ParseIni(File.ReadAllLines(iniPath));
                if (ini2.TryGetValue("INPUT", out var inputSection2)
                    && inputSection2.TryGetValue("KeyScreenshot", out var ks2)
                    && !string.IsNullOrWhiteSpace(ks2))
                    currentScreenshotHotkey = ks2;
            }
            catch { /* use fallback */ }
        }

        var screenshotHotkeyString = currentScreenshotHotkey;
        var screenshotHotkeyBox = new TextBox
        {
            Text = HotkeyManager.FormatHotkeyDisplay(screenshotHotkeyString),
            IsReadOnly = true,
            PlaceholderText = "Click then press a key...",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        ToolTipService.SetToolTip(screenshotHotkeyBox, "Click here then press your desired key. Written to all reshade*.ini files for this game.");

        screenshotHotkeyBox.GotFocus += (s, ev) => screenshotHotkeyBox.Text = "Press a key...";
        screenshotHotkeyBox.KeyDown += (s, ev) =>
        {
            var vk2 = (int)ev.Key;
            if (vk2 == 0 || vk2 == 16 || vk2 == 17 || vk2 == 18) return;
            bool shift2 = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            bool ctrl2  = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            bool alt2   = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            screenshotHotkeyString = HotkeyManager.BuildHotkeyString(vk2, shift2, ctrl2, alt2);
            screenshotHotkeyBox.Text = HotkeyManager.FormatHotkeyDisplay(screenshotHotkeyString);
            ev.Handled = true;
        };
        screenshotHotkeyBox.LostFocus += (s, ev) =>
        {
            if (screenshotHotkeyBox.Text == "Press a key...")
                screenshotHotkeyBox.Text = HotkeyManager.FormatHotkeyDisplay(screenshotHotkeyString);
        };

        var applyScreenshotKeyBtn = new Button
        {
            Content = "Apply",
            FontSize = 12,
            Padding = new Thickness(16, 7, 16, 7),
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        applyScreenshotKeyBtn.Click += (s, ev) =>
        {
            if (string.IsNullOrEmpty(card.InstallPath)) return;
            try
            {
                var iniFiles2 = Directory.EnumerateFiles(card.InstallPath, "reshade*.ini")
                    .Where(f => Path.GetExtension(f).Equals(".ini", StringComparison.OrdinalIgnoreCase)
                             && Path.GetFileNameWithoutExtension(f).StartsWith("reshade", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var file in iniFiles2)
                    AuxInstallService.ApplyScreenshotHotkey(file, screenshotHotkeyString);
                applyScreenshotKeyBtn.Content = "Applied!";
                _crashReporter.Log($"[RsCogButton_Click] Applied screenshot key '{screenshotHotkeyString}' to {iniFiles2.Count} ini file(s) for '{card.GameName}'");
            }
            catch (Exception ex) { card.RsActionMessage = $"❌ {ex.Message}"; }
        };

        var screenshotKeyGrid = new Grid { ColumnSpacing = 8 };
        screenshotKeyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        screenshotKeyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        screenshotHotkeyBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        Grid.SetColumn(screenshotHotkeyBox, 0);
        Grid.SetColumn(applyScreenshotKeyBtn, 1);
        screenshotKeyGrid.Children.Add(screenshotHotkeyBox);
        screenshotKeyGrid.Children.Add(applyScreenshotKeyBtn);

        content.Children.Add(screenshotKeyGrid);

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
                        AuxInstallService.ApplyEngineIniHdrSettings(card.InstallPath, card.EngineIniProjectOverride, card.GameName, card.Source);
                        if (card.InstalledRecord != null) card.InstalledRecord.EngineIniHdr = true;
                        card.ActionMessage = "✅ Engine.ini HDR settings deployed.";
                    }
                    else
                    {
                        AuxInstallService.RemoveEngineIniHdrSettings(card.InstallPath, card.EngineIniProjectOverride, card.GameName, card.Source);
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
                    AuxInstallService.ApplyEngineIniLutSetting(card.InstallPath, card.EngineIniProjectOverride, card.GameName, card.Source);
                    if (card.InstalledRecord != null) card.InstalledRecord.EngineIniLut = true;
                    card.ActionMessage = "✅ LUT Update Every Frame enabled in Engine.ini.";
                }
                else
                {
                    AuxInstallService.RemoveEngineIniLutSetting(card.InstallPath, card.EngineIniProjectOverride, card.GameName, card.Source);
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
        content.Children.Add(new TextBlock
        {
            Text = "Requires NVIDIA App with Overlay and Game Filters enabled.",
            FontSize = 11,
            Foreground = UIFactory.Brush(ResourceKeys.InlineDescriptionBrush),
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 4),
        });

        var rtxHdrCombo = new ComboBox { FontSize = 11, MinWidth = 100 };
        rtxHdrCombo.Items.Add("Off");
        rtxHdrCombo.Items.Add("On");

        var gameNameService = App.Services.GetRequiredService<IGameNameService>();
        // Read live driver state — reflects changes made outside RHI (e.g. NVIDIA App, driver update)
        var dlssPresetServiceCog = App.Services.GetRequiredService<DlssPresetService>();
        bool isRtxHdrEnabled = dlssPresetServiceCog.IsSupported && !string.IsNullOrEmpty(card.InstallPath)
            ? (dlssPresetServiceCog.GetRtxHdrEnable(card.GameName, card.InstallPath) == 0x01)
            : gameNameService.RtxHdrGames.Contains(card.GameName);
        // Sync persisted state to match driver
        if (isRtxHdrEnabled) gameNameService.RtxHdrGames.Add(card.GameName);
        else gameNameService.RtxHdrGames.Remove(card.GameName);
        card.IsRtxHdrEnabled = isRtxHdrEnabled;
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
                // Default to Gamma 2.2 (Contrast = +25, stored = 125) — matches conventional SDR gamma
                var enablePeakNits = ViewModel.Settings.PeakNits > 0 ? ViewModel.Settings.PeakNits : 510;
                // Calculate ITU-correct Middle Grey for Gamma 2.2 at the user's peak nits
                // paperWhite lookup table: (peak, pw nits) — interpolated
                static double Lerp(double a, double b, double t) => a + t * (b - a);
                double enablePaperWhite;
                (double peak, double pw)[] ituTable = { (400,101),(600,138),(800,172),(1000,203),(1500,276),(2000,343) };
                if (enablePeakNits <= ituTable[0].peak) enablePaperWhite = ituTable[0].pw;
                else if (enablePeakNits >= ituTable[^1].peak) enablePaperWhite = ituTable[^1].pw;
                else
                {
                    enablePaperWhite = ituTable[^1].pw;
                    for (int i = 0; i < ituTable.Length - 1; i++)
                    {
                        if (enablePeakNits >= ituTable[i].peak && enablePeakNits <= ituTable[i+1].peak)
                        {
                            double t = (enablePeakNits - ituTable[i].peak) / (ituTable[i+1].peak - ituTable[i].peak);
                            enablePaperWhite = Lerp(ituTable[i].pw, ituTable[i+1].pw, t);
                            break;
                        }
                    }
                }
                var enableMidGrey = (uint)Math.Clamp((int)Math.Round(enablePaperWhite * Math.Pow(0.5, 2.2)), 10, 100);

                dlssPresetService.SetRtxHdrEnable(card.GameName, card.InstallPath, 0x01);
                dlssPresetService.SetRtxHdrPeakBrightness(card.GameName, card.InstallPath, (uint)enablePeakNits);
                dlssPresetService.SetRtxHdrContrast(card.GameName, card.InstallPath, 125);       // Gamma 2.2 (+25)
                dlssPresetService.SetRtxHdrSaturation(card.GameName, card.InstallPath, 75);      // -25 (reduced saturation)
                dlssPresetService.SetRtxHdrMiddleGrey(card.GameName, card.InstallPath, enableMidGrey); // ITU-correct for Gamma 2.2

                CrashReporter.Log($"[RdxCogButton_Click] RTX HDR enabled for '{card.GameName}': PeakNits={enablePeakNits}, Contrast=125 (Gamma 2.2), Sat=75 (-25), MidGrey={enableMidGrey}");
            }
            else
            {
                gameNameService.RtxHdrGames.Remove(card.GameName);
                card.IsRtxHdrEnabled = false;

                // Delete all RTX HDR settings from profile (revert to global/inherited)
                // Some settings (0x00DD48Fx) can't be deleted via NvAPI — write defaults instead
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
        var content = new StackPanel { Spacing = 6 };

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
        var nitsRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var nitsLabel = new TextBlock { Text = $"Peak Brightness: {peakBrightnessDisplay} nits", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush), MinWidth = 175 };
        var nitsWarning = new TextBlock { Text = "⚠ High values may look unnatural", FontSize = 10, Foreground = UIFactory.Brush(ResourceKeys.AccentAmberBrush), VerticalAlignment = VerticalAlignment.Center, Opacity = peakBrightnessDisplay > 600 ? 1.0 : 0.0 };
        nitsRow.Children.Add(nitsLabel);
        nitsRow.Children.Add(nitsWarning);
        var nitsSlider = new Slider { Minimum = 400, Maximum = 2000, StepFrequency = 10, Value = peakBrightnessDisplay, HorizontalAlignment = HorizontalAlignment.Stretch };
        nitsSlider.ValueChanged += (s, ev) =>
        {
            nitsLabel.Text = $"Peak Brightness: {(int)nitsSlider.Value} nits";
            nitsWarning.Opacity = (int)nitsSlider.Value > 600 ? 1.0 : 0.0;
        };
        content.Children.Add(nitsRow);
        content.Children.Add(nitsSlider);

        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 2, 0, 2) });
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

        // ── Gamma preset buttons ──────────────────────────────────────────────
        var gammaPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(0, 2, 0, 4) };
        foreach (var (label, value) in new[] { ("Gamma 2.0", 0), ("Gamma 2.2", 25), ("Gamma 2.4", 50) })
        {
            var btn = new Button
            {
                Content = label,
                FontSize = 11,
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            var capturedValue = value;
            btn.Click += (s, ev) =>
            {
                contrastSlider.Value = capturedValue;
                contrastLabel.Text = ContrastLabel(capturedValue);
            };
            gammaPanel.Children.Add(btn);
        }
        content.Children.Add(gammaPanel);

        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 2, 0, 2) });
        // ── Middle Grey ───────────────────────────────────────────────────────
        // ITU-recommended paper white nits per peak brightness (interpolated for values between table entries)
        // Source: https://www.rtings.com/tv/learn/rtx-hdr (table from community research)
        // Formula: midGreyNits = paperWhiteNits × (0.5 ^ gamma)
        // gamma: 2.0 = contrast 0, 2.2 = contrast +25, 2.4 = contrast +50
        static double CalcPaperWhiteNits(double peakNits)
        {
            // ITU lookup table: (peakNits, paperWhiteNits)
            (double peak, double pw)[] table =
            {
                (400,  101), (600,  138), (800,  172),
                (1000, 203), (1500, 276), (2000, 343),
            };
            if (peakNits <= table[0].peak)  return table[0].pw;
            if (peakNits >= table[^1].peak) return table[^1].pw;
            for (int i = 0; i < table.Length - 1; i++)
            {
                if (peakNits >= table[i].peak && peakNits <= table[i + 1].peak)
                {
                    double t = (peakNits - table[i].peak) / (table[i + 1].peak - table[i].peak);
                    return table[i].pw + t * (table[i + 1].pw - table[i].pw);
                }
            }
            return 203; // fallback
        }
        static int CalcAutoMiddleGrey(double peakNits, int contrastVal)
        {
            double gamma = contrastVal switch { 25 => 2.2, 50 => 2.4, _ => 2.0 };
            // For non-preset contrast values interpolate gamma linearly between anchors
            if (contrastVal != 0 && contrastVal != 25 && contrastVal != 50)
                gamma = 2.0 + (contrastVal / 100.0) * 0.4; // rough linear: 0→2.0, 100→2.4
            var pw = CalcPaperWhiteNits(peakNits);
            var mg = pw * Math.Pow(0.5, gamma);
            return Math.Clamp((int)Math.Round(mg), 10, 100);
        }

        var middleGreyValues = new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 };
        int mgInitial = (currentMiddleGrey >= 10 && currentMiddleGrey <= 100) ? currentMiddleGrey : 50;
        
        // Calculate perceived paperwhite from middle grey and gamma
        // Formula: paperwhite = midGrey / (0.5 ^ gamma)
        int CalcPerceivedPaperwhite(int midGrey, int contrastVal)
        {
            double gamma = contrastVal switch { 25 => 2.2, 50 => 2.4, _ => 2.0 };
            if (contrastVal != 0 && contrastVal != 25 && contrastVal != 50)
                gamma = 2.0 + (contrastVal / 100.0) * 0.4;
            var pw = midGrey / Math.Pow(0.5, gamma);
            return (int)Math.Round(pw);
        }
        
        string MiddleGreyLabel(int val, int contrastVal)
        {
            var perceivedPw = CalcPerceivedPaperwhite(val, contrastVal);
            return $"Middle Grey: {val} ({perceivedPw} nits)";
        }
        
        int mgInitialPw = CalcPerceivedPaperwhite(mgInitial, (int)contrastSlider.Value);
        var mgRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var mgLabel = new TextBlock { Text = MiddleGreyLabel(mgInitial, (int)contrastSlider.Value), FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush), MinWidth = 175 };
        var mgWarning = new TextBlock { Text = "⚠ High values may look washed out", FontSize = 10, Foreground = UIFactory.Brush(ResourceKeys.AccentAmberBrush), VerticalAlignment = VerticalAlignment.Center, Opacity = mgInitialPw > 203 ? 1.0 : 0.0 };
        mgRow.Children.Add(mgLabel);
        mgRow.Children.Add(mgWarning);
        var mgSlider = new Slider { Minimum = 10, Maximum = 100, StepFrequency = 1, Value = mgInitial, HorizontalAlignment = HorizontalAlignment.Stretch };
        mgSlider.ValueChanged += (s, ev) =>
        {
            mgLabel.Text = MiddleGreyLabel((int)mgSlider.Value, (int)contrastSlider.Value);
            mgWarning.Opacity = CalcPerceivedPaperwhite((int)mgSlider.Value, (int)contrastSlider.Value) > 203 ? 1.0 : 0.0;
        };
        // Also update when contrast changes (gamma affects perceived paperwhite)
        contrastSlider.ValueChanged += (s, ev) =>
        {
            mgLabel.Text = MiddleGreyLabel((int)mgSlider.Value, (int)contrastSlider.Value);
            mgWarning.Opacity = CalcPerceivedPaperwhite((int)mgSlider.Value, (int)contrastSlider.Value) > 203 ? 1.0 : 0.0;
        };
        content.Children.Add(mgRow);
        content.Children.Add(mgSlider);

        // Auto button + preset buttons — calculates correct Middle Grey or uses predefined values
        var mgButtonsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Margin = new Thickness(0, 2, 0, 4) };
        var autoMgBtn = new Button
        {
            Content = "Auto",
            FontSize = 11,
            Padding = new Thickness(10, 4, 10, 4),
        };
        ToolTipService.SetToolTip(autoMgBtn, "Calculate Middle Grey from Peak Brightness and Gamma using the ITU formula");
        autoMgBtn.Click += (s, ev) =>
        {
            var autoVal = CalcAutoMiddleGrey((int)nitsSlider.Value, (int)contrastSlider.Value);
            mgSlider.Value = autoVal;
            mgLabel.Text = MiddleGreyLabel(autoVal, (int)contrastSlider.Value);
            mgWarning.Opacity = CalcPerceivedPaperwhite(autoVal, (int)contrastSlider.Value) > 203 ? 1.0 : 0.0;
        };
        mgButtonsPanel.Children.Add(autoMgBtn);
        
        // Separator
        mgButtonsPanel.Children.Add(new TextBlock { Text = "|", FontSize = 11, Foreground = UIFactory.Brush(ResourceKeys.TextTertiaryBrush), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0) });
        
        // Preset buttons for common paperwhite values (100-200 nits range)
        foreach (var presetPw in new[] { 100, 125, 150, 175, 200 })
        {
            var presetBtn = new Button
            {
                Content = presetPw.ToString(),
                FontSize = 11,
                Padding = new Thickness(8, 4, 8, 4),
                MinWidth = 36,
            };
            var capturedPw = presetPw;
            ToolTipService.SetToolTip(presetBtn, $"Set Middle Grey to achieve ~{presetPw} nits paperwhite");
            presetBtn.Click += (s, ev) =>
            {
                // Reverse formula: midGrey = paperwhite × (0.5 ^ gamma)
                double gamma = (int)contrastSlider.Value switch { 25 => 2.2, 50 => 2.4, _ => 2.0 };
                if ((int)contrastSlider.Value != 0 && (int)contrastSlider.Value != 25 && (int)contrastSlider.Value != 50)
                    gamma = 2.0 + ((int)contrastSlider.Value / 100.0) * 0.4;
                var mgVal = Math.Clamp((int)Math.Round(capturedPw * Math.Pow(0.5, gamma)), 10, 100);
                mgSlider.Value = mgVal;
                mgLabel.Text = MiddleGreyLabel(mgVal, (int)contrastSlider.Value);
                mgWarning.Opacity = CalcPerceivedPaperwhite(mgVal, (int)contrastSlider.Value) > 203 ? 1.0 : 0.0;
            };
            mgButtonsPanel.Children.Add(presetBtn);
        }
        content.Children.Add(mgButtonsPanel);

        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 2, 0, 2) });
        // ── Saturation ────────────────────────────────────────────────────────
        string SaturationLabel(int val) => val switch
        {
            -25 => "Saturation: -25 — Neutral Saturation",
            _ => $"Saturation: {(val >= 0 ? "+" : "")}{val}",
        };
        var satLabel = new TextBlock { Text = SaturationLabel(saturationDisplay), FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextPrimaryBrush) };
        var satSlider = new Slider { Minimum = -100, Maximum = 100, StepFrequency = 1, Value = saturationDisplay, HorizontalAlignment = HorizontalAlignment.Stretch };
        satSlider.ValueChanged += (s, ev) => satLabel.Text = SaturationLabel((int)satSlider.Value);
        content.Children.Add(satLabel);
        content.Children.Add(satSlider);

        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 2, 0, 2) });
        // ── Debanding ─────────────────────────────────────────────────────────
        var debandingOptions = new (string name, uint value)[]
        {
            ("No Debanding", 0x06),
            ("Low Debanding", 0x0A),
            ("High Debanding", 0x02),
            ("High Debanding (Indicator)", 0x03),
            ("High Debanding (Indicator + Debug)", 0x23),
        };
        bool isAdmin = VulkanLayerService.IsRunningAsAdmin();
        var debandingCombo = new ComboBox { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Stretch, IsEnabled = isAdmin, Opacity = isAdmin ? 1.0 : 0.4 };
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
        if (!isAdmin)
            content.Children.Add(new TextBlock
            {
                Text = "Requires admin mode to change",
                FontSize = 10,
                Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                Margin = new Thickness(0, -4, 0, 0),
            });

        // ── Default preset buttons ────────────────────────────────────────────
        var defaultsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RHI", "rtx_hdr_defaults.json");

        // Load current defaults (if any) to show whether "Set Default" is available
        bool hasDefaults = File.Exists(defaultsPath);

        var defaultsPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 4, 0, 0) };
        var saveDefaultBtn = new Button
        {
            Content = "Save as Default",
            FontSize = 11,
            Padding = new Thickness(12, 6, 12, 6),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        ToolTipService.SetToolTip(saveDefaultBtn, "Save current slider values as your default RTX HDR preset");
        var setDefaultBtn = new Button
        {
            Content = "Set Default",
            FontSize = 11,
            Padding = new Thickness(12, 6, 12, 6),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsEnabled = hasDefaults,
        };
        ToolTipService.SetToolTip(setDefaultBtn, hasDefaults ? "Apply your saved default preset to the sliders" : "No default saved yet — use 'Save as Default' first");

        saveDefaultBtn.Click += (s, ev) =>
        {
            try
            {
                var defaults = new Dictionary<string, object>
                {
                    ["PeakBrightness"] = (int)nitsSlider.Value,
                    ["Contrast"]       = (int)contrastSlider.Value,
                    ["Saturation"]     = (int)satSlider.Value,
                    ["MiddleGrey"]     = (int)mgSlider.Value,
                    ["Debanding"]      = (int)debandingOptions[debandingCombo.SelectedIndex].value,
                };
                Directory.CreateDirectory(Path.GetDirectoryName(defaultsPath)!);
                File.WriteAllText(defaultsPath, JsonSerializer.Serialize(defaults,
                    new JsonSerializerOptions { WriteIndented = true }));
                setDefaultBtn.IsEnabled = true;
                ToolTipService.SetToolTip(setDefaultBtn, "Apply your saved default preset to the sliders");
                saveDefaultBtn.Content = "Saved!";
            }
            catch (Exception ex) { CrashReporter.Log($"[RtxHdrConfigButton_Click] Failed to save defaults — {ex.Message}"); }
        };

        setDefaultBtn.Click += (s, ev) =>
        {
            try
            {
                var json = File.ReadAllText(defaultsPath);
                var defaults = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (defaults == null) return;

                if (defaults.TryGetValue("PeakBrightness", out var pb)) nitsSlider.Value = Math.Clamp(pb.GetInt32(), 400, 2000);
                if (defaults.TryGetValue("Contrast", out var ct)) contrastSlider.Value = Math.Clamp(ct.GetInt32(), -100, 100);
                if (defaults.TryGetValue("Saturation", out var sat)) satSlider.Value = Math.Clamp(sat.GetInt32(), -100, 100);
                if (defaults.TryGetValue("MiddleGrey", out var mg))
                {
                    var mgVal = Math.Clamp(mg.GetInt32(), 10, 100);
                    mgSlider.Value = mgVal;
                }
                if (defaults.TryGetValue("Debanding", out var db))
                {
                    var dbVal = db.GetInt32();
                    for (int i = 0; i < debandingOptions.Length; i++)
                    {
                        if ((int)debandingOptions[i].value == dbVal) { debandingCombo.SelectedIndex = i; break; }
                    }
                }
            }
            catch (Exception ex) { CrashReporter.Log($"[RtxHdrConfigButton_Click] Failed to apply defaults — {ex.Message}"); }
        };

        defaultsPanel.Children.Add(saveDefaultBtn);
        defaultsPanel.Children.Add(setDefaultBtn);
        content.Children.Add(defaultsPanel);

        // ── Dialog ────────────────────────────────────────────────────────────
        var dialog = new ContentDialog
        {
            Title = "RTX HDR Settings",
            Content = new ScrollViewer { Content = content, MaxHeight = 600, Padding = new Thickness(0, 0, 16, 0) },
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
        var middleGrey = (uint)mgSlider.Value;
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
        bool ulIniExists = !string.IsNullOrEmpty(card.InstallPath)
            && File.Exists(Path.Combine(card.InstallPath, "relimiter.ini"));

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
        if (!ulIniExists)
        {
            targetFpsPanel.Opacity = 0.4;
            targetFpsPanel.IsHitTestVisible = false;
            content.Children.Add(new TextBlock
            {
                Text = "Deploy relimiter.ini to enable these settings",
                FontSize = 10,
                Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
                Margin = new Thickness(0, -2, 0, 0),
            });
        }

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
        if (!ulIniExists)
        {
            dlssHooksPanel.Opacity = 0.4;
            dlssHooksPanel.IsHitTestVisible = false;
        }

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
        var content = new StackPanel { Spacing = 10 };

        // ── Helper: build a 4-column settings grid (label | combo | label | combo) ──
        // Returns the grid; use AddRow() to populate it.
        Grid MakeSettingsGrid()
        {
            var g = new Grid { ColumnSpacing = 12, RowSpacing = 8 };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
            return g;
        }
        void AddRow(Grid g, int row, string leftLabel, ComboBox leftCombo, string? rightLabel = null, ComboBox? rightCombo = null)
        {
            while (g.RowDefinitions.Count <= row) g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var lbl1 = new TextBlock { Text = leftLabel, FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(lbl1, row); Grid.SetColumn(lbl1, 0); g.Children.Add(lbl1);
            leftCombo.FontSize = 12; leftCombo.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(leftCombo, row); Grid.SetColumn(leftCombo, 1); g.Children.Add(leftCombo);
            if (rightLabel != null && rightCombo != null)
            {
                var lbl2 = new TextBlock { Text = rightLabel, FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), VerticalAlignment = VerticalAlignment.Center };
                Grid.SetRow(lbl2, row); Grid.SetColumn(lbl2, 2); g.Children.Add(lbl2);
                rightCombo.FontSize = 12; rightCombo.HorizontalAlignment = HorizontalAlignment.Stretch;
                Grid.SetRow(rightCombo, row); Grid.SetColumn(rightCombo, 3); g.Children.Add(rightCombo);
            }
        }

        Border MakeSeparator() => new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 4, 0, 4) };

        // ── OptiScaler Version — label col 0, combo col 1 (aligned with settings grid) ──
        var versionGrid = new Grid { ColumnSpacing = 12 };
        versionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        versionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
        versionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        versionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });
        var versionLabel = new TextBlock { Text = "OptiScaler Version", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(versionLabel, 0); versionGrid.Children.Add(versionLabel);
        var variantCombo = new ComboBox { ItemsSource = new[] { "Stable", "Nightly" }, SelectedItem = ViewModel.GetOsVariant(card.GameName, card.Source ?? ""), FontSize = 12, HorizontalAlignment = HorizontalAlignment.Stretch };
        ToolTipService.SetToolTip(variantCombo, "Stable uses the official OptiScaler release. Nightly uses the latest daily build.");
        Grid.SetColumn(variantCombo, 1); versionGrid.Children.Add(variantCombo);
        content.Children.Add(versionGrid);

        ContentDialog? osCogDialog = null;
        variantCombo.SelectionChanged += (s, ev) =>
        {
            var selected = variantCombo.SelectedItem as string ?? "Stable";
            ViewModel.SetOsVariant(card.GameName, selected == "Stable" ? null : selected, card.Source ?? "");
            if (card.IsOsInstalled && !string.IsNullOrEmpty(card.InstallPath))
            {
                try { _optiScalerService.Uninstall(card); card.NotifyAll(); }
                catch (Exception ex) { CrashReporter.Log($"[OsCog] Uninstall on channel switch — {ex.Message}"); }
            }
            DispatcherQueue.TryEnqueue(async () => { osCogDialog?.Hide(); await Task.Delay(80); OsCogButton_Click(sender, e); });
        };

        bool isNightly = ViewModel.GetOsVariant(card.GameName, card.Source ?? "") == "Nightly";
        if (isNightly)
        {
            content.Children.Add(MakeSeparator());

            // ── INI value converters ───────────────────────────────────────
            string FgInputToIni(string d) => d switch { "OptiFG (Upscaler)" => "upscaler", "DLSSG via Streamline" => "dlssg", "DLSSG via Nvngx" => "nvngxfg", "FSR 3.1 FG" => "fsrfg", "FSR 3.0 FG" => "fsrfg30", "XeFG" => "xefg", _ => "auto" };
            string IniToFgInput(string v) => v switch { "upscaler" => "OptiFG (Upscaler)", "dlssg" => "DLSSG via Streamline", "nvngxfg" => "DLSSG via Nvngx", "fsrfg" => "FSR 3.1 FG", "fsrfg30" => "FSR 3.0 FG", "xefg" => "XeFG", _ => "Auto (Default)" };
            string FgOutputToIni(string d) => d switch { "FSR FG" => "fsrfg", "DLSSG" => "dlssg", "XeFG" => "xefg", _ => "auto" };
            string IniToFgOutput(string v) => v switch { "fsrfg" => "FSR FG", "dlssg" => "DLSSG", "xefg" => "XeFG", _ => "Auto (Default)" };
            string FgNvngxToIni(string d) => d switch { "Nukem's" => "Nukems", "Enabler" => "Arturs", "FSR 3/4 FG" => "FFX", _ => "None" };
            string IniToFgNvngx(string v) => v switch { "Nukems" => "Nukem's", "Arturs" => "Enabler", "FFX" => "FSR 3/4 FG", _ => "None (Real DLSSG)" };

            // ── Single settings grid for all nightly rows ──────────────────
            var nightlyGrid = MakeSettingsGrid();

            // Row 0: Deploy Streamline | Deploy DLSS Enabler
            var streamlineCombo = new ComboBox { ItemsSource = new[] { "No", "Yes" }, SelectedItem = ViewModel.GetOsDeployStreamline(card.GameName, card.Source ?? "") ? "Yes" : "No" };
            var enablerCombo = new ComboBox { ItemsSource = new[] { "No", "Yes" }, SelectedItem = ViewModel.GetOsDeployDlssEnabler(card.GameName, card.Source ?? "") ? "Yes" : "No", IsEnabled = ViewModel.GetOsDeployStreamline(card.GameName, card.Source ?? "") };
            ToolTipService.SetToolTip(enablerCombo, "Requires Deploy Streamline to be enabled.");
            AddRow(nightlyGrid, 0, "Deploy Streamline", streamlineCombo, "Deploy DLSS Enabler", enablerCombo);

            // Row 1: FG Input | FG Output
            var fgInputCombo = new ComboBox { ItemsSource = new[] { "Auto (Default)", "OptiFG (Upscaler)", "DLSSG via Streamline", "DLSSG via Nvngx", "FSR 3.1 FG", "FSR 3.0 FG", "XeFG" }, SelectedItem = IniToFgInput(ViewModel.GetOsFgInput(card.GameName, card.Source ?? "")) };
            var fgOutputCombo = new ComboBox { ItemsSource = new[] { "Auto (Default)", "FSR FG", "DLSSG", "XeFG" }, SelectedItem = IniToFgOutput(ViewModel.GetOsFgOutput(card.GameName, card.Source ?? "")) };
            AddRow(nightlyGrid, 1, "FG Input", fgInputCombo, "FG Output", fgOutputCombo);

            // Row 2: (spacer on left) | FG Nvngx Replacement
            bool enablerAvail = ViewModel.GetOsDeployStreamline(card.GameName, card.Source ?? "") && ViewModel.GetOsDeployDlssEnabler(card.GameName, card.Source ?? "");
            var nvngxItems = new List<object> { "None (Real DLSSG)", "Nukem's", new ComboBoxItem { Content = "Enabler", IsEnabled = enablerAvail }, "FSR 3/4 FG" };
            var currentNvngxDisplay = IniToFgNvngx(ViewModel.GetOsFgNvngxReplacement(card.GameName, card.Source ?? ""));
            object? nvngxSelected = nvngxItems.FirstOrDefault(i => i is ComboBoxItem cb ? (cb.Content as string) == currentNvngxDisplay : (i as string) == currentNvngxDisplay) ?? nvngxItems[0];
            var fgNvngxCombo = new ComboBox { ItemsSource = nvngxItems, SelectedItem = nvngxSelected };
            ToolTipService.SetToolTip(fgNvngxCombo, "Only relevant when FG Output = DLSSG. Enabler requires Deploy Streamline + Deploy DLSS Enabler.");
            // Add row 2 with only right side populated
            nightlyGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var nvngxLabel = new TextBlock { Text = "FG Nvngx Replacement", FontSize = 12, Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(nvngxLabel, 2); Grid.SetColumn(nvngxLabel, 2); nightlyGrid.Children.Add(nvngxLabel);
            fgNvngxCombo.FontSize = 12; fgNvngxCombo.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetRow(fgNvngxCombo, 2); Grid.SetColumn(fgNvngxCombo, 3); nightlyGrid.Children.Add(fgNvngxCombo);

            bool fgOutputIsDlssg = fgOutputCombo.SelectedItem as string == "DLSSG";
            // Use Opacity on just row 2's elements to avoid layout jumps
            nvngxLabel.Opacity = fgOutputIsDlssg ? 1.0 : 0.35;
            fgNvngxCombo.Opacity = fgOutputIsDlssg ? 1.0 : 0.35;
            fgNvngxCombo.IsHitTestVisible = fgOutputIsDlssg;

            content.Children.Add(nightlyGrid);

            // ── Wire handlers ──────────────────────────────────────────────
            streamlineCombo.SelectionChanged += (s, ev) =>
            {
                bool on = streamlineCombo.SelectedItem as string == "Yes";
                ViewModel.SetOsDeployStreamline(card.GameName, on, card.Source ?? "");
                enablerCombo.IsEnabled = on;
                if (!on) { enablerCombo.SelectedItem = "No"; ViewModel.SetOsDeployDlssEnabler(card.GameName, false, card.Source ?? ""); }
                if (!string.IsNullOrEmpty(card.InstallPath))
                    try { if (on) _optiScalerService.DeployStreamlineToGame(card.InstallPath); else _optiScalerService.RemoveStreamlineFromGame(card.InstallPath); }
                    catch (Exception ex) { CrashReporter.Log($"[OsCog] Streamline deploy — {ex.Message}"); }
            };
            enablerCombo.SelectionChanged += (s, ev) =>
            {
                bool on = enablerCombo.SelectedItem as string == "Yes";
                ViewModel.SetOsDeployDlssEnabler(card.GameName, on, card.Source ?? "");
                if (!string.IsNullOrEmpty(card.InstallPath))
                    try { var d = Path.Combine(card.InstallPath, "OptiScaler"); if (on) _ = _dlssEnablerService.InstallAsync(d); else _dlssEnablerService.Uninstall(d); }
                    catch (Exception ex) { CrashReporter.Log($"[OsCog] DLSS Enabler — {ex.Message}"); }
            };
            fgInputCombo.SelectionChanged += (s, ev) =>
            {
                if (fgInputCombo.SelectedItem is not string sel) return;
                var v = FgInputToIni(sel); ViewModel.SetOsFgInput(card.GameName, v, card.Source ?? "");
                if (!string.IsNullOrEmpty(card.InstallPath)) OptiScalerService.SetOptiScalerIniValue(card.InstallPath, "FrameGen", "FGInput", v);
            };
            fgOutputCombo.SelectionChanged += (s, ev) =>
            {
                if (fgOutputCombo.SelectedItem is not string sel) return;
                var v = FgOutputToIni(sel); ViewModel.SetOsFgOutput(card.GameName, v, card.Source ?? "");
                if (!string.IsNullOrEmpty(card.InstallPath)) OptiScalerService.SetOptiScalerIniValue(card.InstallPath, "FrameGen", "FGOutput", v);
                bool isDlssg = sel == "DLSSG";
                nvngxLabel.Opacity = isDlssg ? 1.0 : 0.35;
                fgNvngxCombo.Opacity = isDlssg ? 1.0 : 0.35;
                fgNvngxCombo.IsHitTestVisible = isDlssg;
            };
            fgNvngxCombo.SelectionChanged += (s, ev) =>
            {
                string? display = fgNvngxCombo.SelectedItem is ComboBoxItem cb ? cb.Content as string : fgNvngxCombo.SelectedItem as string;
                if (display == null) return;
                var v = FgNvngxToIni(display); ViewModel.SetOsFgNvngxReplacement(card.GameName, v, card.Source ?? "");
                if (!string.IsNullOrEmpty(card.InstallPath) && string.Equals(ViewModel.GetOsFgOutput(card.GameName, card.Source ?? ""), "dlssg", StringComparison.OrdinalIgnoreCase))
                    OptiScalerService.SetOptiScalerIniValue(card.InstallPath, "FrameGen", "FGNvngxReplacement", v);
            };

            // ── UE-only Engine.ini settings ────────────────────────────────
            bool isUnreal = card.EngineHint?.Contains("Unreal", StringComparison.OrdinalIgnoreCase) == true;
            if (isUnreal)
            {
                content.Children.Add(MakeSeparator());

                var ueGrid = MakeSettingsGrid();

                var dmvCombo = new ComboBox { ItemsSource = new[] { "Default", "Off" }, SelectedItem = ViewModel.GetOsDilatedMotionVectorsOff(card.GameName, card.Source ?? "") ? "Off" : "Default" };
                ToolTipService.SetToolTip(dmvCombo, "Off: r.NGX.DLSS.DilateMotionVectors=0 + r.Streamline.DilateMotionVectors=0");
                var fsrCombo = new ComboBox { ItemsSource = new[] { "None", "FSR2", "FSR3", "FSR3.1" }, SelectedItem = ViewModel.GetOsFsrCrashFix(card.GameName, card.Source ?? "") };
                ToolTipService.SetToolTip(fsrCombo, "FSR2: r.FidelityFX.FSR2.UseNativeDX12=1\nFSR3: r.FidelityFX.FSR3.UseNativeDX12=1\nFSR3.1: above + r.FidelityFX.FSR3.UseRHI=0");
                AddRow(ueGrid, 0, "Dilated MV", dmvCombo, "FSR Crash Fix", fsrCombo);

                var fgSwapCombo = new ComboBox { ItemsSource = new[] { "Default", "On" }, SelectedItem = ViewModel.GetOsFsrFgSwapchain(card.GameName, card.Source ?? "") ? "On" : "Default" };
                ToolTipService.SetToolTip(fgSwapCombo, "On: r.FidelityFX.FI.OverrideSwapChainDX12=1");
                var upscalerCombo = new ComboBox { ItemsSource = new[] { "Default", "On" }, SelectedItem = ViewModel.GetOsUpscalerPlugin(card.GameName, card.Source ?? "") ? "On" : "Default" };
                ToolTipService.SetToolTip(upscalerCombo, "On: r.AntiAliasingMethod=4 + r.TemporalAA.Upscaler=1");
                AddRow(ueGrid, 1, "FSR-FG Swapchain", fgSwapCombo, "Upscaler Plugin", upscalerCombo);

                content.Children.Add(ueGrid);

                dmvCombo.SelectionChanged += (s, ev) =>
                {
                    bool off = dmvCombo.SelectedItem as string == "Off"; ViewModel.SetOsDilatedMotionVectorsOff(card.GameName, off, card.Source ?? "");
                    if (!string.IsNullOrEmpty(card.InstallPath)) { var keys = new (string, string, string)[] { ("SystemSettings", "r.NGX.DLSS.DilateMotionVectors", "0"), ("SystemSettings", "r.Streamline.DilateMotionVectors", "0") }; try { if (off) AuxInstallService.ApplyEngineIniCustomKeys(card.InstallPath, keys, card.EngineIniProjectOverride, card.GameName, card.Source); else AuxInstallService.RemoveEngineIniCustomKeys(card.InstallPath, keys.Select(k => k.Item2), card.EngineIniProjectOverride, card.GameName, card.Source); } catch { } }
                };
                fsrCombo.SelectionChanged += (s, ev) =>
                {
                    var sel = fsrCombo.SelectedItem as string ?? "None"; var allK = new[] { "r.FidelityFX.FSR2.UseNativeDX12", "r.FidelityFX.FSR3.UseNativeDX12", "r.FidelityFX.FSR3.UseRHI" };
                    if (!string.IsNullOrEmpty(card.InstallPath)) { try { AuxInstallService.RemoveEngineIniCustomKeys(card.InstallPath, allK, card.EngineIniProjectOverride, card.GameName, card.Source); } catch { } if (sel != "None") { var k = sel switch { "FSR2" => new (string, string, string)[] { ("SystemSettings", "r.FidelityFX.FSR2.UseNativeDX12", "1") }, "FSR3" => new (string, string, string)[] { ("SystemSettings", "r.FidelityFX.FSR3.UseNativeDX12", "1") }, _ => new (string, string, string)[] { ("SystemSettings", "r.FidelityFX.FSR3.UseNativeDX12", "1"), ("SystemSettings", "r.FidelityFX.FSR3.UseRHI", "0") } }; try { AuxInstallService.ApplyEngineIniCustomKeys(card.InstallPath, k, card.EngineIniProjectOverride, card.GameName, card.Source); } catch { } } }
                    ViewModel.SetOsFsrCrashFix(card.GameName, sel == "None" ? null : sel, card.Source ?? "");
                };
                fgSwapCombo.SelectionChanged += (s, ev) =>
                {
                    bool on = fgSwapCombo.SelectedItem as string == "On"; ViewModel.SetOsFsrFgSwapchain(card.GameName, on, card.Source ?? "");
                    if (!string.IsNullOrEmpty(card.InstallPath)) { var k = new (string, string, string)[] { ("SystemSettings", "r.FidelityFX.FI.OverrideSwapChainDX12", "1") }; try { if (on) AuxInstallService.ApplyEngineIniCustomKeys(card.InstallPath, k, card.EngineIniProjectOverride, card.GameName, card.Source); else AuxInstallService.RemoveEngineIniCustomKeys(card.InstallPath, k.Select(x => x.Item2), card.EngineIniProjectOverride, card.GameName, card.Source); } catch { } }
                };
                upscalerCombo.SelectionChanged += (s, ev) =>
                {
                    bool on = upscalerCombo.SelectedItem as string == "On"; ViewModel.SetOsUpscalerPlugin(card.GameName, on, card.Source ?? "");
                    if (!string.IsNullOrEmpty(card.InstallPath)) { var k = new (string, string, string)[] { ("SystemSettings", "r.AntiAliasingMethod", "4"), ("SystemSettings", "r.TemporalAA.Upscaler", "1") }; try { if (on) AuxInstallService.ApplyEngineIniCustomKeys(card.InstallPath, k, card.EngineIniProjectOverride, card.GameName, card.Source); else AuxInstallService.RemoveEngineIniCustomKeys(card.InstallPath, k.Select(x => x.Item2), card.EngineIniProjectOverride, card.GameName, card.Source); } catch { } }
                };
            }
        }

        content.Children.Add(MakeSeparator());

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
        osCogDialog = dialog;
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

        // ── Vulkan/OpenGL Present Method ──────────────────────────────────
        content.Children.Add(new Border { Height = 1, Background = UIFactory.Brush(ResourceKeys.BorderDefaultBrush), Margin = new Thickness(0, 4, 0, 0) });
        var presentGrid = new Grid { ColumnSpacing = 12 };
        presentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        presentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110, GridUnitType.Pixel) });

        var presentLabel = new TextBlock
        {
            Text = "Prefer DXGI Swapchain",
            FontSize = 11,
            Foreground = UIFactory.Brush(ResourceKeys.TextSecondaryBrush),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTipService.SetToolTip(presentLabel, "Sets Vulkan/OpenGL Present Method to 'Preferred layered on DXGI Swapchain' in the NVIDIA driver profile. Recommended for DXVK — improves compatibility and HDR support.");
        Grid.SetColumn(presentLabel, 0);
        presentGrid.Children.Add(presentLabel);

        var presentCombo = new ComboBox { FontSize = 11, MinWidth = 100, HorizontalAlignment = HorizontalAlignment.Stretch };
        presentCombo.Items.Add("No");   // 0x00000002 — Auto
        presentCombo.Items.Add("Yes");  // 0x00000001 — Preferred layered on DXGI Swapchain
        var currentPresentMethod = _dlssPresetService.GetVulkanPresentMethod(card.GameName, card.InstallPath ?? "");
        presentCombo.SelectedIndex = currentPresentMethod == 0x00000001 ? 1 : 0;
        presentCombo.SelectionChanged += (s, ev) =>
        {
            uint value = presentCombo.SelectedIndex == 1 ? 0x00000001u : 0x00000002u;
            _dlssPresetService.SetVulkanPresentMethod(card.GameName, card.InstallPath ?? "", value);
        };
        Grid.SetColumn(presentCombo, 1);
        presentGrid.Children.Add(presentCombo);
        content.Children.Add(presentGrid);

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
