using MySqlConnector;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

public abstract class StudentPageBase : UserControl
{
    protected StudentPageBase(AppSession session)
    {
        Session = session;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        Font = UiTheme.Body;
        DoubleBuffered = true;
    }

    protected AppSession Session { get; }

    public virtual Task LoadAsync() => Task.CompletedTask;

    protected async Task SafeLoadAsync()
    {
        try
        {
            await LoadAsync();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    protected static string FriendlyError(Exception ex)
    {
        return ex is MySqlException
            ? "cannot reach the database. start XAMPP (MySQL) and try again."
            : ex.Message;
    }

    protected static TableLayoutPanel Header(string title, string subtitle, params Control[] actions)
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.Background,
            Margin = new Padding(0, 0, 0, 16),
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var text = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
        };
        text.Controls.Add(UiTheme.Label(title, UiTheme.Heading), 0, 0);
        text.Controls.Add(UiTheme.Label(subtitle, UiTheme.Small, UiTheme.Muted), 0, 1);

        var actionPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Margin = new Padding(0),
        };
        foreach (Control action in actions)
        {
            action.Margin = new Padding(6, 0, 0, 0);
            actionPanel.Controls.Add(action);
        }

        header.Controls.Add(text, 0, 0);
        header.Controls.Add(actionPanel, 1, 0);
        return header;
    }
}
