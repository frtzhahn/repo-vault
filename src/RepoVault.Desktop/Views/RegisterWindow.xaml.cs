using System;
using System.Linq;
using System.Windows;
using MySqlConnector;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views;

public partial class RegisterWindow : Window
{
    private readonly AppSession _session;

    public RegisterWindow() : this(new AppSession())
    {
    }

    public RegisterWindow(AppSession session)
    {
        _session = session;
        InitializeComponent();
    }

    public string? RegisteredUsername { get; private set; }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        string idNumber = TxtIdNumber.Text.Trim();
        string username = TxtUsername.Text.Trim();
        string fullName = TxtFullName.Text.Trim();
        string email = TxtEmail.Text.Trim();
        string password = TxtPassword.Password;
        string confirm = TxtConfirmPassword.Password;

        if (idNumber.Length == 0)
        {
            ShowError("Student ID number is required.");
            return;
        }

        if (username.Length == 0)
        {
            ShowError("Username is required.");
            return;
        }

        if (fullName.Length == 0)
        {
            ShowError("Full name is required.");
            return;
        }

        if (email.Length < 3 || !email.Contains('@'))
        {
            ShowError("A valid academic email address is required.");
            return;
        }

        if (password.Length < AuthService.MinPasswordLength)
        {
            ShowError("Password must be at least " + AuthService.MinPasswordLength + " characters.");
            return;
        }

        if (!password.Any(char.IsDigit))
        {
            ShowError("Password needs at least one digit.");
            return;
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            ShowError("Password needs at least one symbol.");
            return;
        }

        if (password != confirm)
        {
            ShowError("Passwords do not match.");
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _session.Auth.RegisterUserAsync(
                idNumber, username, password, fullName, email, UserRole.Student);

            if (!result.Ok || result.User is null)
            {
                ShowError(result.Message);
                return;
            }

            RegisteredUsername = result.User.Username;
            DialogResult = true;
            Close();
        }
        catch (MySqlException)
        {
            ShowError("Cannot reach MariaDB on port 3306. Start XAMPP MySQL and try again.");
        }
        catch (Exception ex)
        {
            ShowError("Registration failed: " + ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowError(string message)
    {
        LblError.Text = message;
        ErrorBanner.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        BtnRegister.IsEnabled = !busy;
        BtnCancel.IsEnabled = !busy;
        BtnRegister.Content = busy ? "Registering..." : "Register";
    }
}
