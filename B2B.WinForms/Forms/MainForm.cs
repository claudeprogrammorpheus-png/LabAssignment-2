using B2B.Core;

namespace B2B.WinForms.Forms;

public class MainForm : Form
{
    public readonly TabControl Tabs = new() { Dock = DockStyle.Fill, Font = Ui.F(10.5f), Padding = new Point(18, 6) };
    public readonly ItemsPanel Items = new();
    public readonly AgentsPanel Agents = new();
    public readonly OrdersPanel Orders = new();
    public readonly DashboardPanel Dashboard = new();

    public MainForm(User admin)
    {
        Text = "B2B Wholesale - Back-office";
        ClientSize = new Size(1280, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Back;
        Font = Ui.F(9.5f);

        var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Ui.Navy };
        header.Controls.Add(new Label { Text = "B2B Wholesale · Warehouse Back-office", Font = Ui.F(14, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(18, 14) });
        var who = new Label { Text = $"{admin.UserName} ({admin.Role})", Font = Ui.F(10), ForeColor = Color.White, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        header.Controls.Add(who);
        header.Resize += (_, _) => who.Location = new Point(header.Width - who.Width - 20, 18);

        foreach (var (title, panel) in new (string, Control)[] { ("  Items  ", Items), ("  Agents  ", Agents), ("  Orders  ", Orders), ("  Dashboard  ", Dashboard) })
        {
            var page = new TabPage(title) { BackColor = Ui.Back, Padding = new Padding(8) };
            panel.Dock = DockStyle.Fill;
            page.Controls.Add(panel);
            Tabs.TabPages.Add(page);
        }
        // Reload data whenever a tab is opened so new web orders / stock changes appear.
        Tabs.SelectedIndexChanged += (_, _) => (Tabs.SelectedTab?.Controls[0] as IReloadable)?.Reload();

        Controls.Add(Tabs);
        Controls.Add(header);
        Load += (_, _) => Items.Reload();
    }
}

public interface IReloadable
{
    void Reload();
}
