// MainWindow.FaqBuilder.cs — Builds the FAQ/Quick Start guide content dynamically.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace RenoDXCommander;

public sealed partial class MainWindow
{
    private bool _faqBuilt;

    /// <summary>
    /// Builds the FAQ panel content. Called once when FAQ is first opened.
    /// </summary>
    private void BuildFaqContent()
    {
        if (_faqBuilt) return;
        _faqBuilt = true;

        var panel = FaqContentPanel;
        panel.Children.Clear();

        // Welcome section
        panel.Children.Add(BuildFaqSection(
            null, "Welcome to RHI", "AccentTealBrush",
            "RHI auto-detects your games and lets you install HDR mods, shaders, frame limiters, and manage NVIDIA driver settings — all from one place. Here's how to get started.",
            null));

        // Step 1: Select a Game
        panel.Children.Add(BuildFaqStep(1,
            "Select a Game",
            "Your games are listed in the sidebar on the left. Click any game to see its details and available actions. Use the filter chips (All Games, Installed, Unreal, etc.) and search box to find specific games.",
            "Tip: Double-click a game to launch it directly. Drag and drop a game's .exe file onto RHI to add games not auto-detected."));

        // Step 2: Install ReShade
        panel.Children.Add(BuildFaqStep(2,
            "Install ReShade",
            "ReShade is required for RenoDX HDR mods to work. Click 'Install ReShade' on the game's detail panel. RHI automatically downloads and installs the correct version with full addon support.",
            "ReShade version can be changed per-game via the game overrides section — choose Stable, Nightly, Legacy, or a custom ReShade DLL.\nVulkan Games: Vulkan games (like Doom Eternal) require admin privileges. RHI will prompt for elevation when needed.\nDrag and drop ReShade preset files (.ini) onto a game to install them automatically."));

        // Step 3: Install RenoDX
        panel.Children.Add(BuildFaqStep(3,
            "Install RenoDX / HDR Mod",
            "The RenoDX row shows if an HDR mod is available. Click it to install. Games with dedicated mods show the mod name; Unreal Engine games can use 'UE-Extended' for native HDR output or the generic 'RenoDX' fallback.",
            "The cog icon next to RenoDX opens advanced settings: Peak Nits and UE-Extended toggle.\nFor games not on the wiki, drag and drop an .addon64 file from the RenoDX Discord directly onto the game in RHI.\nTip: If a game doesn't have a RenoDX mod and isn't Unreal/Unity, you can enable RTX HDR (NVIDIA's SDR-to-HDR conversion) in the RenoDX cog menu instead."));

        // Step 4: Choose Shaders
        panel.Children.Add(BuildFaqStep(4,
            "Choose Shaders (Optional)",
            "Click the 'Shaders/Addons' button in the toolbar, then 'Global Shaders' to select shader packs. Lilium's HDR shader pack is selected by default. These apply to all games with ReShade installed.",
            "Tip: Per-game shaders can be set using the Shaders button on each game's detail card (when ReShade is installed)."));

        // Step 5: Frame Limiters
        panel.Children.Add(BuildFaqStep(5,
            "Frame Limiters (Optional)",
            "ReLimiter and Display Commander are ReShade addons that provide precise frame limiting for VRR displays. Install them from the game's detail panel. ReLimiter is recommended as it's developed by the same team as RHI. Set your target FPS in Settings, per-game via the cog icon, or directly in-game.",
            "VRR cap presets by refresh rate (leave headroom below max for smooth VRR):\n• 60Hz → 59 FPS\n• 120Hz → 116 FPS\n• 144Hz → 138 FPS\n• 165Hz → 157 FPS\n• 240Hz → 224 FPS\n• 360Hz → 324 FPS\nThese values are pre-configured in RHI's FPS dropdown menus."));

        // Step 6: DLSS/Streamline
        panel.Children.Add(BuildFaqStep(6,
            "Update DLSS / Streamline (Optional)",
            "Games with DLSS or Streamline DLLs have a dedicated section on the detail panel showing version info. Click to update to the latest version. Using the newest versions is recommended for best performance and quality. RHI backs up originals automatically so you can restore anytime.",
            "When new DLSS or Streamline versions release, they will appear in RHI automatically. Set your default DLSS preset in Settings. Per-game presets can be changed in the DLSS section on each game's detail panel."));

        // Settings Overview
        panel.Children.Add(BuildFaqInfoSection("Settings",
            "Click 'Settings' in the toolbar to configure defaults for all games:",
            new[]
            {
                "ReLimiter FPS: Default frame rate target",
                "DLSS Preset: Default upscaling preset",
                "NVIDIA Driver Settings: VSync, Low Latency, Power Mode",
                "Peak Nits: Your display's peak brightness for HDR",
                "ReShade Hotkeys: Customize overlay and screenshot keys"
            }));

        // NVIDIA Driver Settings
        panel.Children.Add(BuildFaqInfoSection("NVIDIA Driver Settings",
            "RHI can manage per-game NVIDIA driver profiles. These settings are available directly on each game's detail panel:",
            new[]
            {
                "VSync: On, Off, or Adaptive (Fast Sync)",
                "Low Latency Mode: Ultra, On, or Off",
                "Smooth Motion: Multi Frame Generation (per-game only)",
                "ReBAR: Resizable BAR (requires admin)"
            },
            "Global defaults for VSync, Low Latency, and Power Mode are set in Settings. Per-game overrides are configured directly on each game's detail panel."));

        // Vulkan Games
        panel.Children.Add(BuildFaqSpecialSection("V", "AccentPurpleBrush",
            "Vulkan Games",
            "Vulkan games (shown with a 'Vulkan' badge) use a global ReShade layer installed to C:\\ProgramData\\ReShade. This requires administrator privileges. When you install ReShade on a Vulkan game, RHI will prompt for elevation.",
            "All Vulkan games share the same ReShade installation. Updating ReShade on one Vulkan game updates it for all.\nPer-game RenoDX addons and shaders are still installed individually to each game folder."));

        // Adding Games Manually
        panel.Children.Add(BuildFaqSpecialSection("+", "AccentAmberBrush",
            "Adding Games Manually",
            "If a game isn't auto-detected, drag and drop its .exe file directly onto the RHI window. RHI will add it to your library and detect its engine type.",
            "You can also drag .addon64 files from the RenoDX Discord onto any game to install mods not yet on the wiki."));

        // Updating Everything
        panel.Children.Add(BuildFaqSpecialSection("↑", "AccentGreenBrush",
            "Updating Everything",
            "Click 'Update All' in the toolbar to update all installed components across all games at once. This includes ReShade, RenoDX mods, ReLimiter, Display Commander, and more.",
            "Games with available updates show a green dot in the sidebar. Configure which components are included in 'Update All' from Settings."));

        // Troubleshooting - Full Refresh
        panel.Children.Add(BuildFaqSpecialSection("↻", "AccentBlueBrush",
            "Troubleshooting: Full Refresh",
            "If games are missing, install locations have changed, or DLSS/Streamline files have been added or removed, use 'Full Refresh' in Settings to rescan your entire library from scratch.",
            "Full Refresh clears the cached game list and re-detects everything. Use it when the normal Refresh button doesn't pick up changes."));

        // System Tray
        panel.Children.Add(BuildFaqSpecialSection("◰", "AccentPurpleBrush",
            "System Tray",
            "RHI can minimize to the system tray instead of closing. Right-click the tray icon to quickly launch recent games without opening the main window.",
            "Enable 'Close to System Tray' in Settings to keep RHI running in the background. The tray icon provides quick access to your most recently played games. RHI automatically checks for updates every 4 hours while running, so everything stays up to date."));

        // Need More Help
        panel.Children.Add(BuildFaqLinksSection());
    }


    private Border BuildFaqSection(string? badge, string title, string titleBrush, string description, string? tip)
    {
        var stack = new StackPanel { Spacing = 10 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        if (badge != null)
        {
            var badgeBorder = new Border
            {
                Background = (Brush)Application.Current.Resources[titleBrush],
                CornerRadius = new CornerRadius(12),
                Width = 24,
                Height = 24
            };
            badgeBorder.Child = new TextBlock
            {
                Text = badge,
                FontSize = 12,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            header.Children.Add(badgeBorder);
        }
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources[titleBrush],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20
        });

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqStep(int step, string title, string description, string? tip)
    {
        var stack = new StackPanel { Spacing = 8 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var badgeBorder = new Border
        {
            Background = (Brush)Application.Current.Resources["AccentTealBrush"],
            CornerRadius = new CornerRadius(12),
            Width = 24,
            Height = 24
        };
        badgeBorder.Child = new TextBlock
        {
            Text = step.ToString(),
            FontSize = 12,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        header.Children.Add(badgeBorder);
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20,
            Margin = new Thickness(32, 0, 0, 0)
        });

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(32, 4, 0, 0),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqInfoSection(string title, string description, string[] bullets, string? tip = null)
    {
        var stack = new StackPanel { Spacing = 8 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var badgeBorder = new Border
        {
            Background = (Brush)Application.Current.Resources["AccentBlueBrush"],
            CornerRadius = new CornerRadius(12),
            Width = 24,
            Height = 24
        };
        badgeBorder.Child = new TextBlock
        {
            Text = "?",
            FontSize = 12,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        header.Children.Add(badgeBorder);
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20,
            Margin = new Thickness(32, 0, 0, 0)
        });

        var bulletStack = new StackPanel { Spacing = 6, Margin = new Thickness(32, 4, 0, 0) };
        foreach (var bullet in bullets)
        {
            bulletStack.Children.Add(new TextBlock
            {
                Text = $"• {bullet}",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            });
        }
        stack.Children.Add(bulletStack);

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(32, 4, 0, 0),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqSpecialSection(string badge, string badgeBrush, string title, string description, string? tip)
    {
        var stack = new StackPanel { Spacing = 8 };

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var badgeBorder = new Border
        {
            Background = (Brush)Application.Current.Resources[badgeBrush],
            CornerRadius = new CornerRadius(12),
            Width = 24,
            Height = 24
        };
        var badgeFg = badgeBrush == "AccentAmberBrush"
            ? new SolidColorBrush(Microsoft.UI.Colors.Black)
            : new SolidColorBrush(Microsoft.UI.Colors.White);
        badgeBorder.Child = new TextBlock
        {
            Text = badge,
            FontSize = badge.Length > 1 ? 11 : 14,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = badgeFg,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        header.Children.Add(badgeBorder);
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["TextPrimaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });
        stack.Children.Add(header);

        stack.Children.Add(new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20,
            Margin = new Thickness(32, 0, 0, 0)
        });

        if (tip != null)
        {
            var tipBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["SurfaceToolbarBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(32, 4, 0, 0),
                BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
                BorderThickness = new Thickness(1)
            };
            tipBorder.Child = new TextBlock
            {
                Text = tip,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 11,
                Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
                LineHeight = 18
            };
            stack.Children.Add(tipBorder);
        }

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["BorderSubtleBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }


    private Border BuildFaqLinksSection()
    {
        var stack = new StackPanel { Spacing = 10 };

        stack.Children.Add(new TextBlock
        {
            Text = "Need More Help?",
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = (Brush)Application.Current.Resources["AccentTealBrush"]
        });

        stack.Children.Add(new TextBlock
        {
            Text = "Support is available on Discord — join the community for help, mod updates, and discussion.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextSecondaryBrush"],
            LineHeight = 20
        });

        var linksStack = new StackPanel { Spacing = 6 };

        var discordLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://discord.gg/ultraplus"),
            Padding = new Thickness(0)
        };
        discordLink.Content = new TextBlock
        {
            Text = "Join the Ultra+ Discord (main community)",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(discordLink);

        var renodxDiscordLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://discord.gg/renodx"),
            Padding = new Thickness(0)
        };
        renodxDiscordLink.Content = new TextBlock
        {
            Text = "RenoDX Discord (mod development)",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(renodxDiscordLink);

        var wikiLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://github.com/clshortfuse/renodx/wiki/Mods"),
            Padding = new Thickness(0)
        };
        wikiLink.Content = new TextBlock
        {
            Text = "Browse the RenoDX Mod Wiki",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(wikiLink);

        var githubLink = new HyperlinkButton
        {
            NavigateUri = new Uri("https://github.com/RankFTW/RHI"),
            Padding = new Thickness(0)
        };
        githubLink.Content = new TextBlock
        {
            Text = "RHI GitHub — Report issues or request features",
            Foreground = (Brush)Application.Current.Resources["AccentBlueBrush"],
            FontSize = 12
        };
        linksStack.Children.Add(githubLink);

        stack.Children.Add(linksStack);

        return new Border
        {
            Background = (Brush)Application.Current.Resources["SurfaceRaisedBrush"],
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20, 16, 20, 16),
            BorderBrush = (Brush)Application.Current.Resources["AccentTealBorderBrush"],
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }
}
