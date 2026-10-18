using MySqlConnector;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class RegisterForm : Form
{
    private readonly AppSession _session;

    public RegisterForm(AppSession session)
    {
        _session = session;
        InitializeComponent();
    }

    public string? RegisteredUsername { get; private set; }

    private async void btnRegister_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput())
        {
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _session.Auth.RegisterUserAsync(
                txtIdNumber.Text.Trim(), txtUsername.Text.Trim(), txtPassword.Text,
                txtFullName.Text.Trim(), txtEmail.Text.Trim(), UserRole.Student);
            if (!result.Ok)
            {
                MessageBox.Show(this, result.Message, "Registration Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RegisteredUsername = result.User!.Username;
            MessageBox.Show(this, "student account created. you can now sign in.",
                "Registration Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (MySqlException)
        {
            MessageBox.Show(this, "cannot reach the database. start xampp (mysql) and try again.",
                "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "registration failed: " + ex.Message,
                "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private bool ValidateInput()
    {
        bool valid = true;
        valid &= CheckField(txtIdNumber, txtIdNumber.Text.Trim().Length > 0, "id number is required.");
        valid &= CheckField(txtUsername, txtUsername.Text.Trim().Length > 0, "username is required.");
        valid &= CheckField(txtFullName, txtFullName.Text.Trim().Length > 0, "full name is required.");

        string email = txtEmail.Text.Trim();
        valid &= CheckField(txtEmail, email.Contains('@') && email.Length >= 3, "a valid email is required.");

        string password = txtPassword.Text;
        string passwordError = "";
        if (password.Length < AuthService.MinPasswordLength)
        {
            passwordError = "password must be at least " + AuthService.MinPasswordLength + " characters.";
        }
        else if (!password.Any(char.IsDigit))
        {
            passwordError = "password needs at least one digit.";
        }
        else if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            passwordError = "password needs at least one symbol.";
        }

        valid &= CheckField(txtPassword, passwordError.Length == 0, passwordError);

        bool match = password.Length > 0 && password == txtConfirmPassword.Text;
        valid &= CheckField(txtConfirmPassword, match, "passwords do not match.");

        return valid;
    }

    private bool CheckField(Control control, bool ok, string message)
    {
        errorProvider.SetError(control, ok ? "" : message);
        return ok;
    }

    private void SetBusy(bool busy)
    {
        btnRegister.Enabled = !busy;
        btnCancel.Enabled = !busy;
        btnRegister.Text = busy ? "Registering..." : "&Register";
        UseWaitCursor = busy;
    }
}
