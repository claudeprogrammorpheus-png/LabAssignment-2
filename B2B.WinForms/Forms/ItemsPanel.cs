using B2B.Core;
using Microsoft.EntityFrameworkCore;

namespace B2B.WinForms.Forms;

/// <summary>Task 1.2 - CRUD for Items, including stock receipts.</summary>
public class ItemsPanel : UserControl, IReloadable
{
    public readonly DataGridView Grid = Ui.Grid();
    public readonly TextBox Search = new() { Width = 240, Font = Ui.F(10), PlaceholderText = "Search item / size" };
    public readonly TextBox NameBox = new();
    public readonly TextBox SizeBox = new();
    public readonly NumericUpDown PriceBox = new() { Maximum = 1_000_000_000, Increment = 1000, ThousandsSeparator = true };
    public readonly NumericUpDown StockBox = new() { Maximum = 1_000_000, ThousandsSeparator = true };
    public readonly NumericUpDown ReceiveBox = new() { Maximum = 100_000, ThousandsSeparator = true };
    private int? _id;

    public ItemsPanel()
    {
        var newBtn = Ui.Btn("New", Color.White, 80);
        var saveBtn = Ui.Btn("Save", Ui.Blue, 80);
        var delBtn = Ui.Btn("Delete", Ui.Red, 80);
        var recvBtn = Ui.Btn("+ Receive stock", Ui.Green, 140);
        newBtn.Click += (_, _) => Clear();
        saveBtn.Click += (_, _) => Save();
        delBtn.Click += (_, _) => Delete();
        recvBtn.Click += (_, _) => Receive();
        Search.TextChanged += (_, _) => Reload();

        var detail = new Panel { Dock = DockStyle.Right, Width = 380, BackColor = Color.White, Padding = new Padding(0) };
        var form = Ui.Form(("Item name", NameBox), ("Pack size", SizeBox), ("Price (VND)", PriceBox), ("Stock qty", StockBox), ("Receive qty", ReceiveBox));
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 96, Padding = new Padding(14, 8, 0, 0), BackColor = Color.White };
        buttons.Controls.AddRange([newBtn, saveBtn, delBtn, recvBtn]);
        detail.Controls.Add(form);
        detail.Controls.Add(buttons);
        detail.Controls.Add(new Label { Text = "Item details", Dock = DockStyle.Top, Height = 38, Font = Ui.F(11, FontStyle.Bold), Padding = new Padding(12, 10, 0, 0), BackColor = Color.White });

        Grid.SelectionChanged += (_, _) => ShowSelected();
        Grid.CellFormatting += (_, e) =>
        {
            if (Grid.Columns[e.ColumnIndex].Name == "Stock" && e.Value is int q && q <= 20)
            { e.CellStyle!.ForeColor = Ui.Red; e.CellStyle.Font = Ui.F(9.5f, FontStyle.Bold); }
        };

        var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0) };
        gridHost.Controls.Add(Grid);
        Controls.Add(gridHost);
        Controls.Add(detail);
        Controls.Add(Ui.Bar(Ui.Lbl("Search:"), Search, Ui.Lbl("Low stock (≤ 20) is shown in red.", 9, FontStyle.Italic, Color.Gray)));
    }

    public void Reload()
    {
        using var db = Program.Db();
        var q = Search.Text.Trim();
        Grid.DataSource = db.Items.AsNoTracking()
            .Where(i => q == "" || i.ItemName.Contains(q) || i.Size.Contains(q))
            .OrderBy(i => i.ItemID)
            .Select(i => new { i.ItemID, i.ItemName, i.Size, i.Price, Stock = i.StockQuantity }).ToList();
        Ui.Money(Grid, "Price");
        Grid.Columns["ItemID"]!.FillWeight = 30;
        Grid.Columns["ItemName"]!.FillWeight = 160;
    }

    private void ShowSelected()
    {
        if (Grid.CurrentRow?.DataBoundItem is null) return;
        dynamic r = Grid.CurrentRow.DataBoundItem;
        _id = r.ItemID;
        NameBox.Text = r.ItemName;
        SizeBox.Text = r.Size;
        PriceBox.Value = r.Price;
        StockBox.Value = r.Stock;
        ReceiveBox.Value = 0;
    }

    private void Clear()
    {
        _id = null;
        NameBox.Clear(); SizeBox.Clear();
        PriceBox.Value = StockBox.Value = ReceiveBox.Value = 0;
        NameBox.Focus();
    }

    public void Save()
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(SizeBox.Text)) { Ui.Error(this, "Item name and pack size are required."); return; }
        if (PriceBox.Value <= 0) { Ui.Error(this, "Price must be greater than 0."); return; }
        using var db = Program.Db();
        var item = _id is null ? new Item() : db.Items.Find(_id)!;
        item.ItemName = NameBox.Text.Trim();
        item.Size = SizeBox.Text.Trim();
        item.Price = PriceBox.Value;
        item.StockQuantity = (int)StockBox.Value;
        if (_id is null) db.Items.Add(item);
        db.SaveChanges();
        Reload();
        Select(item.ItemID);
    }

    public void Receive()
    {
        if (_id is null || ReceiveBox.Value <= 0) { Ui.Error(this, "Select an item and enter the received quantity."); return; }
        using var db = Program.Db();
        // Atomic increment so concurrent web orders are never overwritten.
        db.Items.Where(i => i.ItemID == _id).ExecuteUpdate(s => s.SetProperty(i => i.StockQuantity, i => i.StockQuantity + (int)ReceiveBox.Value));
        var id = _id.Value;
        Reload();
        Select(id);
    }

    private void Delete()
    {
        if (_id is null) return;
        if (MessageBox.Show(this, $"Delete item '{NameBox.Text}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        using var db = Program.Db();
        if (db.OrderDetails.Any(d => d.ItemID == _id)) { Ui.Error(this, "This item appears in existing orders and cannot be deleted (set its stock to 0 instead)."); return; }
        db.Items.Where(i => i.ItemID == _id).ExecuteDelete();
        Clear();
        Reload();
    }

    public void Select(int id)
    {
        foreach (DataGridViewRow row in Grid.Rows)
            if (row.Cells["ItemID"].Value is int v && v == id) { Grid.CurrentCell = row.Cells[1]; row.Selected = true; ShowSelected(); break; }
    }
}
