using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Forms;

public partial class MainForm : Form
{
    private readonly AppSession _session;
    private int _pushCount;

    public MainForm(AppSession session)
    {
        _session = session;
        InitializeComponent();

        var user = session.CurrentUser;
        if (user is not null)
        {
            lblUserStatus.Text = $"Logged in: {user.FullName} ({user.Role})";
            lblWelcome.Text = $"Welcome, {user.FullName}!";
            lblIdentity.Text = $"{user.Username} - {user.Role} - {user.IdNumber}";
            lblRoleHint.Text = user.Role == UserRole.Faculty
                ? "faculty workspace: course management, grading queue, and repo bin will dock here in phase 2."
                : "student workspace: your teams, repositories, commits, and evaluations will dock here in phase 2.";

            if (user.Role == UserRole.Student)
            {
                pnlWorkspace.Controls.Clear();
                var studentWorkspace = new Controls.StudentWorkspaceControl(_session)
                {
                    Dock = DockStyle.Fill
                };
                pnlWorkspace.Controls.Add(studentWorkspace);
            }
        }

        lblStats.Text = $"storage: {session.ReposDirectory}  |  server port: {session.ServerPort}";
        UpdateServerUi();
    }

    public bool LogoutRequested { get; private set; }

    private async void btnServerToggle_Click(object? sender, EventArgs e)
    {
        if (_session.RunningServer is { IsRunning: true })
        {
            await StopServerAsync();
        }
        else
        {
            await StartServerAsync();
        }
    }

    private async void mnuStartServer_Click(object? sender, EventArgs e)
    {
        await StartServerAsync();
    }

    private async void mnuStopServer_Click(object? sender, EventArgs e)
    {
        await StopServerAsync();
    }

    private async Task StartServerAsync()
    {
        if (_session.RunningServer is { IsRunning: true })
        {
            return;
        }

        int port = _session.ServerPort;
        GitHttpServer server = CreateServer($"http://+:{port}/");
        try
        {
            await server.StartAsync();
        }
        catch (HttpListenerException)
        {
            server.Dispose();

            // wildcard binding needs admin rights or a urlacl rule, so retry on loopback only
            server = CreateServer($"http://127.0.0.1:{port}/");
            try
            {
                await server.StartAsync();
            }
            catch (HttpListenerException ex)
            {
                server.Dispose();
                MessageBox.Show(this,
                    "could not start the git server on port " + port + ": " + ex.Message +
                    Environment.NewLine + Environment.NewLine +
                    "if the port is already taken, close the other process and try again. " +
                    "for LAN binding, run scripts/enable-lan-port.bat as administrator on the host machine.",
                    "Git Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowNotification("LAN binding not permitted; server is running on 127.0.0.1 only. " +
                             "run scripts/enable-lan-port.bat as administrator for LAN access.");
        }

        server.PushCompleted += Server_PushCompleted;
        _session.RunningServer = server;
        UpdateServerUi();
        ShowNotification("git server started at http://" + LanHelper.GetPreferredLanIpv4Address() + ":" + port + "/");
    }

    private async Task StopServerAsync()
    {
        GitHttpServer? server = _session.RunningServer;
        if (server is null)
        {
            return;
        }

        server.PushCompleted -= Server_PushCompleted;
        _session.RunningServer = null;
        try
        {
            await server.StopAsync();
        }
        finally
        {
            server.Dispose();
        }

        UpdateServerUi();
        ShowNotification("git server stopped.");
    }

    private GitHttpServer CreateServer(string prefix)
    {
        return new GitHttpServer(
            _session.Db, _session.Auth, _session.Rbac, _session.Repos,
            _session.ServerPort, prefix);
    }

    private void Server_PushCompleted(object? sender, PushCompletedEventArgs e)
    {
        if (IsDisposed || !IsHandleCreated)
        {
            return;
        }

        try
        {
            BeginInvoke((Action)(() =>
            {
                _pushCount++;
                lblPushCounter.Text = "Pushes Received: " + _pushCount;
                ShowNotification($"push to repo #{e.RepoId} on {e.BranchRef} received at {e.PushedAt.ToLocalTime():HH:mm:ss}.");

                if (pnlWorkspace.Controls.OfType<Controls.StudentWorkspaceControl>().FirstOrDefault() is { } sw)
                {
                    sw.HandlePushNotification(e.RepoId);
                }
            }));
        }
        catch (InvalidOperationException)
        {
            // the form closed while the push notification was in flight
        }
    }

    private void UpdateServerUi()
    {
        bool running = _session.RunningServer is { IsRunning: true };
        btnServerToggle.Text = running ? "Stop Git Server" : "Start Git Server";
        mnuStartServer.Enabled = !running;
        mnuStopServer.Enabled = running;

        string host = LanHelper.GetPreferredLanIpv4Address();
        lblServerStatus.Text = running
            ? $"Server: Running at http://{host}:{_session.ServerPort}/"
            : "Server: Stopped";
    }

    private void ShowNotification(string text)
    {
        lblNotification.Text = text;
        notifyTimer.Stop();
        notifyTimer.Start();
    }

    private void NotifyTimer_Tick(object? sender, EventArgs e)
    {
        notifyTimer.Stop();
        lblNotification.Text = "";
    }

    private void mnuCopyUrl_Click(object? sender, EventArgs e)
    {
        string url = $"http://{LanHelper.GetPreferredLanIpv4Address()}:{_session.ServerPort}/";
        try
        {
            Clipboard.SetText(url);
            ShowNotification($"server url copied: {url} (clone urls look like {url}repos/<slug>.git)");
        }
        catch (ExternalException)
        {
            MessageBox.Show(this, "could not access the clipboard.", "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void mnuViewLanIp_Click(object? sender, EventArgs e)
    {
        string ip = LanHelper.GetPreferredLanIpv4Address();
        MessageBox.Show(this,
            "LAN address: " + ip + Environment.NewLine +
            "clone url example: " + LanHelper.BuildCloneUrl("your-repo", _session.ServerPort, ip) + Environment.NewLine +
            Environment.NewLine +
            "teammates must run scripts/enable-lan-port.bat once (as administrator) on this machine before " +
            "the git server can accept LAN connections.",
            "LAN Address", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void mnuSeed_Click(object? sender, EventArgs e)
    {
        UseWaitCursor = true;
        try
        {
            SeederResult result = await _session.Seeder.SeedAllAsync(_session.ReposDirectory);
            string summary = $"seeded {result.ReposSeeded} repositories and indexed {result.CommitsIndexed} new commits.";
            ShowNotification(summary);
            MessageBox.Show(this,
                summary + Environment.NewLine + Environment.NewLine +
                "storage: " + _session.ReposDirectory,
                "Seed Workspaces", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (pnlWorkspace.Controls.OfType<Controls.StudentWorkspaceControl>().FirstOrDefault() is { } sw)
            {
                await sw.RefreshDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "seeding failed: " + ex.Message, "Seed Workspaces",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void mnuOpenStorage_Click(object? sender, EventArgs e)
    {
        try
        {
            Directory.CreateDirectory(_session.ReposDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = _session.ReposDirectory,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "could not open the storage folder: " + ex.Message, "Repo Vault",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void mnuAbout_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(this,
            "Repo Vault" + Environment.NewLine +
            "UCC BSCS 2A capstone project" + Environment.NewLine + Environment.NewLine +
            "self-hosted academic git archive with commit history, code metrics, " +
            "peer discussion, and rubric evaluation.",
            "About Repo Vault", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void mnuLogout_Click(object? sender, EventArgs e)
    {
        DialogResult confirm = MessageBox.Show(this, "log out of repo vault?", "Logout",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        await StopServerAsync();
        _session.Reset();
        LogoutRequested = true;
        Close();
    }

    private void mnuExit_Click(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        GitHttpServer? server = _session.RunningServer;
        if (server is not null)
        {
            server.PushCompleted -= Server_PushCompleted;
            _session.RunningServer = null;
            try
            {
                server.StopAsync().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // shutdown must never block the window from closing
            }

            server.Dispose();
        }

        base.OnFormClosing(e);
    }
}
