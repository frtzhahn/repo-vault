using RepoVault.Desktop.AppState;
using RepoVault.Desktop.Forms;

namespace RepoVault.Desktop;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var session = new AppSession();

        // show login, then the shell, and loop back to login when the user logs out
        while (true)
        {
            using var login = new LoginForm(session);
            if (login.ShowDialog() != DialogResult.OK)
            {
                break;
            }

            using var main = new MainForm(session);
            Application.Run(main);
            if (!main.LogoutRequested)
            {
                break;
            }
        }
    }
}
