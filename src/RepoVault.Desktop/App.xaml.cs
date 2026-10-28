using System.Windows;
using System.Windows.Media;
using RepoVault.Desktop.AppState;
using RepoVault.Desktop.Views;

namespace RepoVault.Desktop;

public partial class App : Application
{
    public static bool IsDarkMode { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        ApplyTheme(false);

        var session = new AppSession();

        // loop login and main workstation until user exits
        while (true)
        {
            var login = new LoginWindow(session);
            if (login.ShowDialog() != true || !session.IsAuthenticated)
            {
                break;
            }

            var main = new MainWindow(session);
            MainWindow = main;
            main.ShowDialog();
            if (!main.LogoutRequested)
            {
                break;
            }
        }

        Shutdown();
    }

    public static void ToggleTheme()
    {
        ApplyTheme(!IsDarkMode);
    }

    public static void ApplyTheme(bool dark)
    {
        IsDarkMode = dark;
        if (Current is null)
        {
            return;
        }

        if (dark)
        {
            // gruvbox dark soft
            SetBrush("BgCanvasBrush", "#32302F");
            SetBrush("BgSurfaceBrush", "#3C3836");
            SetBrush("BgElevatedBrush", "#504945");
            SetBrush("BgHoverBrush", "#665C54");
            SetBrush("BorderSubtleBrush", "#665C54");
            SetBrush("TextPrimaryBrush", "#EBDBB2");
            SetBrush("TextMutedBrush", "#A89984");
            SetBrush("AccentPrimaryBrush", "#83A598");
            SetBrush("AccentSuccessBrush", "#8EC07C");
            SetBrush("AccentWarningBrush", "#FABD2F");
            SetBrush("AccentDangerBrush", "#D65D0E");
            SetBrush("AccentBadgeBrush", "#D3869B");
            SetBrush("TextOnAccentBrush", "#282828");
        }
        else
        {
            // gruvbox light
            SetBrush("BgCanvasBrush", "#EBDBB2");
            SetBrush("BgSurfaceBrush", "#F2E5BC");
            SetBrush("BgElevatedBrush", "#FBF1C7");
            SetBrush("BgHoverBrush", "#D5C4A1");
            SetBrush("BorderSubtleBrush", "#BDAE93");
            SetBrush("TextPrimaryBrush", "#3C3836");
            SetBrush("TextMutedBrush", "#665C54");
            SetBrush("AccentPrimaryBrush", "#458588");
            SetBrush("AccentSuccessBrush", "#689D6A");
            SetBrush("AccentWarningBrush", "#D79921");
            SetBrush("AccentDangerBrush", "#CC241D");
            SetBrush("AccentBadgeBrush", "#B16286");
            SetBrush("TextOnAccentBrush", "#FBF1C7");
        }
    }

    private static void SetBrush(string key, string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        Current.Resources[key] = brush;
    }
}
