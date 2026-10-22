using RepoVault.Core.Models;

namespace RepoVault.Desktop.Views;

internal static class UiTheme
{
    public static readonly Color Background = Color.White;
    public static readonly Color Sidebar = Color.FromArgb(248, 248, 248);
    public static readonly Color Surface = Color.FromArgb(250, 250, 250);
    public static readonly Color Border = Color.FromArgb(222, 222, 222);
    public static readonly Color Text = Color.FromArgb(28, 28, 28);
    public static readonly Color Muted = Color.FromArgb(110, 110, 110);
    public static readonly Color Selected = Color.FromArgb(234, 234, 234);
    public static readonly Color Primary = Color.FromArgb(28, 28, 28);
    public static readonly Color Badge = Color.FromArgb(240, 240, 240);

    public static readonly Font Body = new("Segoe UI", 9.5f);
    public static readonly Font Small = new("Segoe UI", 8.5f);
    public static readonly Font SemiBold = new("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font Heading = new("Segoe UI", 20f, FontStyle.Bold);
    public static readonly Font Section = new("Segoe UI", 11f, FontStyle.Bold);
    public static readonly Font Mono = new("Consolas", 9.5f);

    public static Label Label(string text, Font? font = null, Color? color = null)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            Font = font ?? Body,
            ForeColor = color ?? Text,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
        };
    }

    public static Label BadgeLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            Font = Small,
            ForeColor = Text,
            BackColor = Badge,
            Padding = new Padding(8, 3, 8, 3),
            Margin = new Padding(6, 0, 0, 0),
        };
    }

    public static Button Button(string text, int width = 100, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Primary : Background,
            ForeColor = primary ? Color.White : Text,
            Font = Body,
            Cursor = Cursors.Hand,
            Margin = new Padding(0),
            UseVisualStyleBackColor = false,
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = primary ? Primary : Border;
        button.FlatAppearance.MouseOverBackColor = primary
            ? Color.FromArgb(55, 55, 55)
            : Color.FromArgb(245, 245, 245);
        button.FlatAppearance.MouseDownBackColor = primary
            ? Color.FromArgb(70, 70, 70)
            : Color.FromArgb(232, 232, 232);
        return button;
    }

    public static Button NavButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Width = 178,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = Sidebar,
            ForeColor = Text,
            Font = Body,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 4),
            TabStop = false,
            UseVisualStyleBackColor = false,
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Selected;
        button.FlatAppearance.MouseDownBackColor = Selected;
        return button;
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackgroundColor = Background,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 34,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            GridColor = Border,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            Font = Body,
            AllowUserToOrderColumns = false,
            AllowUserToResizeColumns = true,
        };
        grid.RowTemplate.Height = 32;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = Muted,
            Font = SemiBold,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            SelectionBackColor = Surface,
            SelectionForeColor = Muted,
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Background,
            ForeColor = Text,
            Font = Body,
            SelectionBackColor = Selected,
            SelectionForeColor = Text,
            Padding = new Padding(8, 0, 0, 0),
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(252, 252, 252),
            SelectionBackColor = Selected,
            SelectionForeColor = Text,
        };
        return grid;
    }

    public static CardPanel Card()
    {
        return new CardPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Surface,
            Padding = new Padding(14),
            Margin = new Padding(0),
        };
    }

    public static DataGridViewTextBoxColumn Column(
        string header, string property, int fillWeight = 100, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = property,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.NotSortable,
        };
        if (format is not null)
        {
            column.DefaultCellStyle.Format = format;
        }

        return column;
    }

    public static string FormatUtc(DateTime? value)
    {
        if (value is null)
        {
            return "-";
        }

        DateTime utc = value.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();
        return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    public static string FormatUtcShort(DateTime? value)
    {
        if (value is null)
        {
            return "-";
        }

        DateTime utc = value.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();
        return utc.ToLocalTime().ToString("yyyy-MM-dd");
    }

    public static string RepoStatus(RepositoryRecord repo)
    {
        return repo.IsLocked ? "Locked" : repo.LifecycleStatus == LifecycleStatus.Active ? "Active" : "Trashed";
    }
}

internal sealed class CardPanel : Panel
{
    public CardPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(UiTheme.Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
