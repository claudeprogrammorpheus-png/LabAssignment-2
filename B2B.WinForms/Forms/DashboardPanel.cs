using System.Configuration;
using B2B.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.WinForms.Forms;

/// <summary>Task 1.4 - dashboard / advanced filter: best sellers, low-stock warning, revenue by agent.</summary>
public class DashboardPanel : UserControl, IReloadable
{
    public readonly DateTimePicker From = new() { Format = DateTimePickerFormat.Short, Width = 135, Font = Ui.F(10) };
    public readonly DateTimePicker To = new() { Format = DateTimePickerFormat.Short, Width = 135, Font = Ui.F(10) };
    public readonly ComboBox AgentFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230, Font = Ui.F(10) };
    public readonly NumericUpDown Threshold = new() { Width = 70, Maximum = 10_000, Font = Ui.F(10) };
    public readonly NumericUpDown TopN = new() { Width = 60, Minimum = 1, Maximum = 100, Value = 10, Font = Ui.F(10) };
    public readonly DataGridView BestGrid = Ui.Grid();
    public readonly DataGridView LowGrid = Ui.Grid();
    public readonly DataGridView RevenueGrid = Ui.Grid();
    private readonly Label _kRevenue = Kpi(), _kOrders = Kpi(), _kLow = Kpi(), _kBest = Kpi();
    private bool _loadingAgents;

    public DashboardPanel()
    {
        From.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-2);
        To.Value = DateTime.Today;
        Threshold.Value = int.TryParse(ConfigurationManager.AppSettings["LowStockThreshold"], out var t) ? t : 20;
        var run = Ui.Btn("Apply filter", Ui.Blue, 110);
        run.Click += (_, _) => Reload();
        AgentFilter.SelectedIndexChanged += (_, _) => { if (!_loadingAgents) Reload(); };

        var kpis = new TableLayoutPanel { Dock = DockStyle.Top, Height = 92, ColumnCount = 4, Padding = new Padding(0, 6, 0, 6) };
        for (int i = 0; i < 4; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        kpis.Controls.Add(Card("Revenue in period (VND)", _kRevenue, Ui.Green), 0, 0);
        kpis.Controls.Add(Card("Orders in period", _kOrders, Ui.Blue), 1, 0);
        kpis.Controls.Add(Card("Low-stock items", _kLow, Ui.Red), 2, 0);
        kpis.Controls.Add(Card("Best-selling item", _kBest, Color.DarkOrange), 3, 0);

        var grids = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
        grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        grids.Controls.Add(Box("Best-selling items (by quantity)", BestGrid), 0, 0);
        grids.Controls.Add(Box("⚠ Low-stock warning", LowGrid), 1, 0);
        grids.Controls.Add(Box("Total revenue by agent", RevenueGrid), 2, 0);
        LowGrid.CellFormatting += (_, e) =>
        {
            if (LowGrid.Columns[e.ColumnIndex].Name == "Stock" && e.Value is int q)
            { e.CellStyle!.ForeColor = q == 0 ? Color.White : Ui.Red; e.CellStyle.BackColor = q == 0 ? Ui.Red : Ui.Amber; e.CellStyle.Font = Ui.F(9.5f, FontStyle.Bold); }
        };

        Controls.Add(grids);
        Controls.Add(kpis);
        Controls.Add(Ui.Bar(Ui.Lbl("From"), From, Ui.Lbl("To"), To, Ui.Lbl("Agent"), AgentFilter, Ui.Lbl("Top"), TopN, Ui.Lbl("Low stock ≤"), Threshold, run));
    }

    public void Reload()
    {
        using var db = Program.Db();
        if (AgentFilter.Items.Count == 0)
        {
            _loadingAgents = true;
            AgentFilter.Items.Add(new AgentChoice(null, "All agents"));
            foreach (var a in db.Agents.AsNoTracking().OrderBy(a => a.AgentName)) AgentFilter.Items.Add(new AgentChoice(a.AgentID, a.AgentName));
            AgentFilter.SelectedIndex = 0;
            _loadingAgents = false;
        }
        var reports = new ReportService(db);
        var agentId = (AgentFilter.SelectedItem as AgentChoice)?.Id;

        var best = reports.BestSelling(From.Value.Date, To.Value.Date, (int)TopN.Value);
        BestGrid.DataSource = best.Select(b => new { b.ItemName, Sold = b.QuantitySold, b.Revenue }).ToList();
        Ui.Money(BestGrid, "Revenue");

        var low = reports.LowStock((int)Threshold.Value);
        LowGrid.DataSource = low.Select(i => new { i.ItemID, Item = i.ItemName, Stock = i.StockQuantity }).ToList();
        LowGrid.Columns["ItemID"]!.FillWeight = 35;

        var rev = reports.RevenueByAgent(From.Value.Date, To.Value.Date, agentId);
        RevenueGrid.DataSource = rev.Select(r => new { Agent = r.AgentName, r.Orders, r.Revenue }).ToList();
        Ui.Money(RevenueGrid, "Revenue");
        RevenueGrid.Columns["Orders"]!.FillWeight = 40;

        _kRevenue.Text = rev.Sum(r => r.Revenue).ToString("N0");
        _kOrders.Text = rev.Sum(r => r.Orders).ToString();
        _kLow.Text = low.Count.ToString();
        _kBest.Text = best.FirstOrDefault()?.ItemName ?? "-";
    }

    private record AgentChoice(int? Id, string Name) { public override string ToString() => Name; }

    private static Label Kpi() => new() { Dock = DockStyle.Fill, Font = Ui.F(15, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };

    private static Control Card(string title, Label value, Color accent)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 8, 0), Padding = new Padding(14, 6, 8, 4) };
        value.ForeColor = accent;
        p.Controls.Add(value);
        p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 22, Font = Ui.F(9), ForeColor = Color.Gray });
        p.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent });
        return p;
    }

    private static Control Box(string title, DataGridView g)
    {
        var p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0), BackColor = Color.White };
        p.Controls.Add(g);
        p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 38, Font = Ui.F(10.5f, FontStyle.Bold), Padding = new Padding(8, 9, 0, 0) });
        return p;
    }
}
