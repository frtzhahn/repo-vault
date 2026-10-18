using MySqlConnector;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class LoginForm : Form
{
    private readonly AppSession _session;

    public LoginForm(AppSession session)
    {
        _session = session;
        InitializeComponent();
        cboDemoAccount.SelectedIndex = 0;
    }

    private async void btnLogin_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput())
        {
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _session.Auth.AuthenticateAsync(txtUsername.Text.Trim(), txtPassword.Text);
            if (!result.Ok || result.User is null)
            {
                lblStatus.Text = result.Message;
                return;
            }

            _session.SetCurrentUser(result.User);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (MySqlException)
        {
            lblStatus.Text = "cannot reach the database. start xampp (mysql) and try again.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "login failed: " + ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void btnQuickFill_Click(object? sender, EventArgs e)
    {
        string demo = cboDemoAccount.Text.Split('(')[0].Trim();
        if (demo.Length == 0)
        {
            return;
        }

        txtUsername.Text = demo;
        txtPassword.Text = "Password123!";
        lblStatus.Text = "demo credentials filled. click sign in.";
        ActiveControl = btnLogin;
    }

    private void lnkRegister_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        using var register = new RegisterForm(_session);
        if (register.ShowDialog(this) == DialogResult.OK && register.RegisteredUsername is not null)
        {
            txtUsername.Text = register.RegisteredUsername;
            txtPassword.Clear();
            lblStatus.Text = "account created. sign in with your new password.";
        }
    }

    private bool ValidateInput()
    {
        bool usernameOk = txtUsername.Text.Trim().Length > 0;
        bool passwordOk = txtPassword.Text.Length > 0;
        errorProvider.SetError(txtUsername, usernameOk ? "" : "username is required.");
        errorProvider.SetError(txtPassword, passwordOk ? "" : "password is required.");
        return usernameOk && passwordOk;
    }

    private void SetBusy(bool busy)
    {
        btnLogin.Enabled = !busy;
        btnQuickFill.Enabled = !busy;
        cboDemoAccount.Enabled = !busy;
        btnLogin.Text = busy ? "Signing in..." : "&Sign In";
        UseWaitCursor = busy;
    }
}
