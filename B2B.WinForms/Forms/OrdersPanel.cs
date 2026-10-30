using B2B.Core;
using B2B.Data;

namespace B2B.WinForms.Forms;

/// <summary>Task 1.2 - order management (incoming web orders, status updates) and Task 1.3 shipping manifest.</summary>
public class OrdersPanel : UserControl, IReloadable
{
    public readonly DataGridView Grid = Ui.Grid();
    public readonly DataGridView Lines = Ui.Grid();
    public readonly ComboBox StatusFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, Font = Ui.F(10) };
    public readonly ComboBox NextStatus = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, Font = Ui.F(10) };
    private readonly Label _info = Ui.Lbl("", 10, FontStyle.Bold, Ui.Navy);
    private List<Order> _orders = new();

    public Order? Selected => Grid.CurrentRow?.Index is int i && i >= 0 && i < _orders.Count ? _orders[i] : null;

    public OrdersPanel()
    {
        StatusFilter.Items.Add("All statuses");
        StatusFilter.Items.AddRange(OrderStatuses.All);
        StatusFilter.SelectedIndex = 0;
        StatusFilter.SelectedIndexChanged += (_, _) => Reload();

        var refresh = Ui.Btn("Refresh", Color.White, 90);
        refresh.Click += (_, _) => Reload();
        var apply = Ui.Btn("Update status", Ui.Blue, 130);
        apply.Click += (_, _) => ApplyStatus();
        var manifest = Ui.Btn("Print Shipping Manifest", Ui.Green, 200);
        manifest.Click += (_, _) => PrintManifest();

        Grid.SelectionChanged += (_, _) => ShowLines();
        Grid.CellFormatting += (_, e) =>
        {
            if (Grid.Columns[e.ColumnIndex].Name != "Status" || e.Value is not string s) return;
            e.CellStyle!.ForeColor = s switch { OrderStatuses.Pending => Color.DarkOrange, OrderStatuses.Cancelled => Color.Gray, OrderStatuses.Delivered => Ui.Green, _ => Ui.Blue };
            e.CellStyle.Font = Ui.F(9.5f, FontStyle.Bold);
        };

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 330, BackColor = Ui.Back };
        split.Panel1.Controls.Add(Grid);
        var linesBox = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        linesBox.Controls.Add(Lines);
        linesBox.Controls.Add(Ui.Bar(_info));
        split.Panel2.Controls.Add(linesBox);

        Controls.Add(split);
        Controls.Add(Ui.Bar(Ui.Lbl("Status:"), StatusFilter, refresh, Ui.Lbl("   Change selected order to:"), NextStatus, apply, manifest));
    }

    public void Reload()
    {
        int? keep = Selected?.OrderID;
        using var db = Program.Db();
        var st = StatusFilter.SelectedIndex <= 0 ? null : (string)StatusFilter.SelectedItem!;
        _orders = new OrderService(db).GetOrders(status: st);
        Grid.DataSource = _orders.Select(o => new
        {
            Order = o.OrderID, Date = o.OrderDate, Agent = o.Agent!.AgentName, Lines = o.OrderDetails.Count,
            Cartons = o.OrderDetails.Sum(d => d.Quantity), Total = o.TotalAmount, Status = o.OrderStatus,
        }).ToList();
        Ui.Money(Grid, "Total");
        Grid.Columns["Date"]!.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        Grid.Columns["Agent"]!.FillWeight = 180;
        if (keep is int id) SelectOrder(id);
        ShowLines();
    }

    public void SelectOrder(int id)
    {
        int i = _orders.FindIndex(o => o.OrderID == id);
        if (i >= 0) Grid.CurrentCell = Grid.Rows[i].Cells[0];
    }

    private void ShowLines()
    {
        var o = Selected;
        NextStatus.Items.Clear();
        if (o is null) { Lines.DataSource = null; _info.Text = ""; return; }
        Lines.DataSource = o.OrderDetails.Select(d => new
        {
            d.ItemID, Item = d.Item!.ItemName, Size = d.Item.Size, d.Quantity, Unit = d.UnitAmount, Amount = d.LineTotal,
        }).ToList();
        Ui.Money(Lines, "Unit", "Amount");
        _info.Text = $"Order #{o.OrderID} · {o.Agent!.AgentName} · {o.Agent.Phone} · Total {o.TotalAmount:N0} VND";
        NextStatus.Items.AddRange(OrderWorkflow.AllowedNext(o.OrderStatus).ToArray<object>());
        if (NextStatus.Items.Count > 0) NextStatus.SelectedIndex = 0;
    }

    public void ApplyStatus()
    {
        if (Selected is not { } o || NextStatus.SelectedItem is not string to) { Ui.Error(this, "Select an order and a new status."); return; }
        try
        {
            using var db = Program.Db();
            new OrderService(db).ChangeStatus(o.OrderID, to);
            Reload();
            SelectOrder(o.OrderID);
        }
        catch (OrderException ex) { Ui.Error(this, ex.Message); }
    }

    public string? PrintManifest(bool open = true)
    {
        if (Selected is not { } o) { Ui.Error(this, "Select an order first."); return null; }
        var path = ShippingManifest.Export(o);
        if (open) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        return path;
    }
}
