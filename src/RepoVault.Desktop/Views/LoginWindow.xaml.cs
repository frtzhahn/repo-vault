using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlConnector;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly AppSession _session;

    public LoginWindow() : this(new AppSession())
    {
    }

    public LoginWindow(AppSession session)
    {
        _session = session;
        InitializeComponent();
        UpdateThemeButtonLabel();
    }

    private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        App.ToggleTheme();
        UpdateThemeButtonLabel();
    }

    private void UpdateThemeButtonLabel()
    {
        BtnThemeToggle.Content = App.IsDarkMode ? "Theme: Dark" : "Theme: Light";
    }

    private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            BtnLogin_Click(sender, e);
        }
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string username = TxtUsername.Text.Trim();
        string password = TxtPassword.Password;

        if (username.Length == 0 || password.Length == 0)
        {
            ShowStatus("Enter both username and password to sign in.", true);
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _session.Auth.AuthenticateAsync(username, password);
            if (!result.Ok || result.User is null)
            {
                ShowStatus(result.Message, true);
                return;
            }

            _session.SetCurrentUser(result.User);
            DialogResult = true;
            Close();
        }
        catch (MySqlException)
        {
            ShowStatus("Cannot reach MariaDB on port 3306. Start XAMPP MySQL and try again.", true);
        }
        catch (Exception ex)
        {
            ShowStatus("Sign in failed: " + ex.Message, true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void BtnQuickFill_Click(object sender, RoutedEventArgs e)
    {
        string raw = (CboDemoAccount.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? CboDemoAccount.Text;
        string demo = raw.Split('(')[0].Trim();
        if (demo.Length == 0)
        {
            return;
        }

        TxtUsername.Text = demo;
        TxtPassword.Password = "Password123!";
        ShowStatus("Demo credentials loaded for " + demo + ". Click Sign In to continue.", false);
        BtnLogin.Focus();
    }

    private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
    {
        var reg = new RegisterWindow(_session)
        {
            Owner = this
        };

        if (reg.ShowDialog() == true && !string.IsNullOrWhiteSpace(reg.RegisteredUsername))
        {
            TxtUsername.Text = reg.RegisteredUsername;
            TxtPassword.Password = string.Empty;
            ShowStatus("Student account created for " + reg.RegisteredUsername + ". Sign in with your password.", false);
            TxtPassword.Focus();
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        LblStatus.Text = message;
        StatusBanner.BorderBrush = isError
            ? (System.Windows.Media.Brush)FindResource("AccentDangerBrush")
            : (System.Windows.Media.Brush)FindResource("AccentPrimaryBrush");
        StatusBanner.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        BtnLogin.IsEnabled = !busy;
        BtnQuickFill.IsEnabled = !busy;
        BtnCreateAccount.IsEnabled = !busy;
        CboDemoAccount.IsEnabled = !busy;
        BtnLogin.Content = busy ? "Signing in..." : "Sign In";
    }
}
