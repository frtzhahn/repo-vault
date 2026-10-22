using RepoVault.Core.Models;
using RepoVault.Desktop.AppState;

namespace RepoVault.Desktop.Views.Student;

public sealed class DiscussionBoardView : UserControl
{
    private readonly AppSession _session;
    private readonly FlowLayoutPanel _comments;
    private readonly TextBox _composer;
    private readonly Button _postButton;
    private readonly Label _status;
    private int? _repoId;

    public DiscussionBoardView(AppSession session)
    {
        _session = session;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        Font = UiTheme.Body;
        DoubleBuffered = true;

        _comments = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = UiTheme.Background,
            Padding = new Padding(0, 0, 6, 0),
            Margin = new Padding(0),
        };
        _comments.Resize += (_, _) => ResizeCards();

        _composer = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            Height = 58,
            Font = UiTheme.Body,
            BorderStyle = BorderStyle.FixedSingle,
            ScrollBars = ScrollBars.Vertical,
            PlaceholderText = "Write a comment...",
            Margin = new Padding(0),
        };
        _postButton = UiTheme.Button("Post Comment", 118, true);
        _postButton.Click += async (_, _) => await PostAsync();
        _status = UiTheme.Label("", UiTheme.Small, UiTheme.Muted);

        var composerCard = UiTheme.Card();
        composerCard.Margin = new Padding(0, 10, 0, 0);
        composerCard.Padding = new Padding(12);
        var composerLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0),
        };
        composerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        composerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        composerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        composerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        composerLayout.Controls.Add(_composer, 0, 0);
        composerLayout.SetColumnSpan(_composer, 2);
        _postButton.Anchor = AnchorStyles.Right;
        _postButton.Margin = new Padding(10, 0, 0, 0);
        composerLayout.Controls.Add(_status, 0, 1);
        composerLayout.Controls.Add(_postButton, 1, 1);
        composerCard.Controls.Add(composerLayout);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.Background,
            Margin = new Padding(0),
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120f));
        root.Controls.Add(_comments, 0, 0);
        root.Controls.Add(composerCard, 0, 1);
        Controls.Add(root);

        SetComposerEnabled(false);
    }

    public async Task LoadRepositoryAsync(int repoId)
    {
        _repoId = repoId;
        _status.Text = "";
        _composer.Clear();
        SetComposerEnabled(true);
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _comments.SuspendLayout();
        foreach (Control control in _comments.Controls.Cast<Control>().ToList())
        {
            control.Dispose();
        }

        _comments.Controls.Clear();

        if (_repoId is null)
        {
            _comments.Controls.Add(UiTheme.Label("Select a repository to read its discussion.", UiTheme.Body, UiTheme.Muted));
            _comments.ResumeLayout();
            return;
        }

        try
        {
            List<DiscussionCommentViewDto> comments =
                await _session.Discussions.ListCommentViewsByRepoAsync(_repoId.Value, false);

            if (IsDisposed)
            {
                return;
            }

            if (comments.Count == 0)
            {
                _comments.Controls.Add(UiTheme.Label("No comments yet. Start the thread below.", UiTheme.Body, UiTheme.Muted));
            }
            else
            {
                foreach (DiscussionCommentViewDto comment in comments)
                {
                    _comments.Controls.Add(BuildCommentCard(comment));
                }
            }
        }
        catch (Exception ex)
        {
            if (IsDisposed)
            {
                return;
            }

            _comments.Controls.Add(UiTheme.Label("Could not load comments: " + ex.Message, UiTheme.Body, UiTheme.Muted));
        }

        _comments.ResumeLayout();
        ResizeCards();
    }

    private Control BuildCommentCard(DiscussionCommentViewDto comment)
    {
        int width = CardWidth();
        var card = new CardPanel
        {
            BackColor = UiTheme.Surface,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 8),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(width, 0),
            MaximumSize = new Size(width, 0),
        };

        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0),
        };

        var meta = new FlowLayoutPanel
        {
            AutoSize = true,
            WrapContents = true,
            BackColor = UiTheme.Surface,
            Margin = new Padding(0),
        };
        meta.Controls.Add(UiTheme.Label(comment.AuthorFullName, UiTheme.SemiBold));
        var role = UiTheme.Label(comment.AuthorRole.ToString(), UiTheme.Small, UiTheme.Muted);
        role.Margin = new Padding(8, 0, 0, 0);
        meta.Controls.Add(role);
        var date = UiTheme.Label(UiTheme.FormatUtc(comment.CreatedAt), UiTheme.Small, UiTheme.Muted);
        date.Margin = new Padding(8, 0, 0, 0);
        meta.Controls.Add(date);
        if (comment.IsModeratedHidden)
        {
            meta.Controls.Add(UiTheme.BadgeLabel("Hidden by moderator"));
        }

        var body = UiTheme.Label(comment.CommentBody, UiTheme.Body);
        body.MaximumSize = new Size(width - 28, 0);
        body.Margin = new Padding(0, 6, 0, 0);

        stack.Controls.Add(meta);
        stack.Controls.Add(body);
        card.Controls.Add(stack);
        return card;
    }

    private int CardWidth()
    {
        int scroll = _comments.VerticalScroll.Visible ? 22 : 0;
        return Math.Max(280, _comments.ClientSize.Width - 8 - scroll);
    }

    private void ResizeCards()
    {
        int width = CardWidth();
        foreach (Control control in _comments.Controls)
        {
            control.MinimumSize = new Size(width, 0);
            control.MaximumSize = new Size(width, 0);
            control.Width = width;
        }
    }

    private async Task PostAsync()
    {
        if (_repoId is null)
        {
            return;
        }

        if (_session.CurrentActor is null)
        {
            _status.Text = "not signed in.";
            return;
        }

        string body = _composer.Text.Trim();
        if (body.Length == 0)
        {
            _status.Text = "write a comment first.";
            return;
        }

        _postButton.Enabled = false;
        _status.Text = "posting...";
        try
        {
            var result = await _session.Discussions.AddCommentAsync(
                _session.CurrentActor, _repoId.Value, body);
            if (IsDisposed)
            {
                return;
            }

            if (!result.Ok)
            {
                _status.Text = result.Message;
                return;
            }

            _composer.Clear();
            _status.Text = "comment posted.";
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            if (IsDisposed)
            {
                return;
            }

            _status.Text = "could not post: " + ex.Message;
        }
        finally
        {
            _postButton.Enabled = true;
        }
    }

    private void SetComposerEnabled(bool enabled)
    {
        _composer.Enabled = enabled;
        _postButton.Enabled = enabled;
    }
}
