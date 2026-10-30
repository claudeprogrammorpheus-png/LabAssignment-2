namespace B2B.WinForms.Forms;

/// <summary>Shared look &amp; feel for the back-office screens.</summary>
internal static class Ui
{
    public static readonly Color Navy = Color.FromArgb(0x1F, 0x4E, 0x79);
    public static readonly Color Blue = Color.FromArgb(0x2E, 0x75, 0xB6);
    public static readonly Color Back = Color.FromArgb(0xF4, 0xF7, 0xFB);
    public static readonly Color Red = Color.FromArgb(0xC0, 0x39, 0x2B);
    public static readonly Color Green = Color.FromArgb(0x2E, 0x8B, 0x57);
    public static readonly Color Amber = Color.FromArgb(0xFF, 0xF2, 0xCC);

    public static Font F(float size, FontStyle s = FontStyle.Regular) => new("Segoe UI", size, s);

    public static Button Btn(string text, Color? back = null, int w = 110)
    {
        var b = new Button
        {
            Text = text, Width = w, Height = 34, FlatStyle = FlatStyle.Flat, Font = F(9.5f, FontStyle.Bold),
            BackColor = back ?? Blue, ForeColor = back == Color.White ? Navy : Color.White, Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowOnly, MinimumSize = new Size(w, 34),
        };
        b.FlatAppearance.BorderColor = back == Color.White ? Navy : b.BackColor;
        return b;
    }

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false, ColumnHeadersHeight = 34, RowTemplate = { Height = 28 }, Font = F(9.5f),
        };
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Navy, ForeColor = Color.White, Font = F(9.5f, FontStyle.Bold), SelectionBackColor = Navy };
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(0xEA, 0xF1, 0xF8);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0xBD, 0xD7, 0xEE);
        g.DefaultCellStyle.SelectionForeColor = Color.Black;
        return g;
    }

    public static void Money(DataGridView g, params string[] cols)
    {
        foreach (var c in cols)
            if (g.Columns[c] is { } col) { col.DefaultCellStyle.Format = "N0"; col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; }
    }

    public static Label Lbl(string text, float size = 9.5f, FontStyle s = FontStyle.Regular, Color? color = null) =>
        new() { Text = text, AutoSize = true, Font = F(size, s), ForeColor = color ?? Color.FromArgb(0x1F, 0x2D, 0x3D), Margin = new Padding(0, 8, 6, 0) };

    public static FlowLayoutPanel Bar(params Control[] cs)
    {
        var p = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(10, 7, 10, 0), BackColor = Color.White, WrapContents = false };
        p.Controls.AddRange(cs);
        return p;
    }

    /// <summary>Two-column label/editor layout used by the CRUD detail panels.</summary>
    public static TableLayoutPanel Form(params (string label, Control editor)[] rows)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(14), AutoScroll = true, BackColor = Color.White };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var (label, editor) in rows)
        {
            editor.Dock = DockStyle.Fill;
            editor.Font = F(10);
            t.Controls.Add(Lbl(label));
            t.Controls.Add(editor);
        }
        return t;
    }

    public static void Error(IWin32Window owner, string msg) => MessageBox.Show(owner, msg, "B2B Back-office", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Info(IWin32Window owner, string msg) => MessageBox.Show(owner, msg, "B2B Back-office", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
