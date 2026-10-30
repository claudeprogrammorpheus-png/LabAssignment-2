using B2B.Core;
using Microsoft.EntityFrameworkCore;

namespace B2B.WinForms.Forms;

/// <summary>Task 1.2 - CRUD for Agents and registration of their portal login.</summary>
public class AgentsPanel : UserControl, IReloadable
{
    public readonly DataGridView Grid = Ui.Grid();
    public readonly TextBox Search = new() { Width = 240, Font = Ui.F(10), PlaceholderText = "Search name / phone" };
    public readonly TextBox NameBox = new();
    public readonly TextBox AddressBox = new();
    public readonly TextBox PhoneBox = new();
    public readonly TextBox LoginBox = new() { PlaceholderText = "portal user name" };
    public readonly TextBox EmailBox = new() { PlaceholderText = "e-mail" };
    public readonly TextBox PasswordBox = new() { PlaceholderText = "new password (leave empty to keep)", UseSystemPasswordChar = true };
    public readonly CheckBox LockBox = new() { Text = "Account locked" };
    private int? _id;

    public AgentsPanel()
    {
        var newBtn = Ui.Btn("New", Color.White, 80);
        var saveBtn = Ui.Btn("Save", Ui.Blue, 80);
        var delBtn = Ui.Btn("Delete", Ui.Red, 80);
        newBtn.Click += (_, _) => Clear();
        saveBtn.Click += (_, _) => Save();
        delBtn.Click += (_, _) => Delete();
        Search.TextChanged += (_, _) => Reload();

        var detail = new Panel { Dock = DockStyle.Right, Width = 400, BackColor = Color.White };
        var form = Ui.Form(("Agent name", NameBox), ("Address", AddressBox), ("Phone", PhoneBox),
                           ("Portal login", LoginBox), ("E-mail", EmailBox), ("Password", PasswordBox), ("Status", LockBox));
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(14, 8, 0, 0), BackColor = Color.White };
        buttons.Controls.AddRange([newBtn, saveBtn, delBtn]);
        detail.Controls.Add(form);
        detail.Controls.Add(buttons);
        detail.Controls.Add(new Label { Text = "Agent & portal account", Dock = DockStyle.Top, Height = 38, Font = Ui.F(11, FontStyle.Bold), Padding = new Padding(12, 10, 0, 0), BackColor = Color.White });

        Grid.SelectionChanged += (_, _) => ShowSelected();
        var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0) };
        gridHost.Controls.Add(Grid);
        Controls.Add(gridHost);
        Controls.Add(detail);
        Controls.Add(Ui.Bar(Ui.Lbl("Search:"), Search));
    }

    public void Reload()
    {
        using var db = Program.Db();
        var q = Search.Text.Trim();
        var users = db.Users.AsNoTracking().Where(u => u.AgentID != null).ToList();
        Grid.DataSource = db.Agents.AsNoTracking()
            .Where(a => q == "" || a.AgentName.Contains(q) || a.Phone.Contains(q))
            .OrderBy(a => a.AgentID)
            .Select(a => new { a.AgentID, a.AgentName, a.Address, a.Phone, Orders = a.Orders.Count })
            .AsEnumerable()
            .Select(a =>
            {
                var u = users.FirstOrDefault(x => x.AgentID == a.AgentID);
                return new { a.AgentID, a.AgentName, a.Address, a.Phone, a.Orders, Login = u?.UserName ?? "-", Locked = u?.Lock ?? false };
            }).ToList();
        Grid.Columns["AgentID"]!.FillWeight = 30;
        Grid.Columns["Orders"]!.FillWeight = 35;
        Grid.Columns["Locked"]!.FillWeight = 35;
        Grid.Columns["Address"]!.FillWeight = 170;
    }

    private void ShowSelected()
    {
        if (Grid.CurrentRow?.DataBoundItem is null) return;
        dynamic r = Grid.CurrentRow.DataBoundItem;
        _id = r.AgentID;
        NameBox.Text = r.AgentName;
        AddressBox.Text = r.Address;
        PhoneBox.Text = r.Phone;
        using var db = Program.Db();
        var u = db.Users.AsNoTracking().FirstOrDefault(x => x.AgentID == _id);
        LoginBox.Text = u?.UserName ?? "";
        EmailBox.Text = u?.Email ?? "";
        LockBox.Checked = u?.Lock ?? false;
        PasswordBox.Clear();
    }

    private void Clear()
    {
        _id = null;
        foreach (var t in new[] { NameBox, AddressBox, PhoneBox, LoginBox, EmailBox, PasswordBox }) t.Clear();
        LockBox.Checked = false;
        NameBox.Focus();
    }

    public void Save()
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(AddressBox.Text) || string.IsNullOrWhiteSpace(PhoneBox.Text))
        { Ui.Error(this, "Name, address and phone are required."); return; }
        using var db = Program.Db();
        if (db.Agents.Any(a => a.Phone == PhoneBox.Text.Trim() && a.AgentID != _id)) { Ui.Error(this, "Another agent already uses this phone number."); return; }

        var agent = _id is null ? new Agent() : db.Agents.Find(_id)!;
        agent.AgentName = NameBox.Text.Trim();
        agent.Address = AddressBox.Text.Trim();
        agent.Phone = PhoneBox.Text.Trim();
        if (_id is null) db.Agents.Add(agent);

        // Optional portal account (registration of a new distributor agent).
        var login = LoginBox.Text.Trim();
        if (login != "")
        {
            var user = _id is null ? null : db.Users.FirstOrDefault(u => u.AgentID == _id);
            if (db.Users.Any(u => (u.UserName == login || u.Email == EmailBox.Text.Trim()) && u.UserID != (user == null ? 0 : user.UserID)))
            { Ui.Error(this, "User name or e-mail already exists."); return; }
            if (user is null)
            {
                if (PasswordBox.Text.Length < 6) { Ui.Error(this, "A new portal account needs a password of at least 6 characters."); return; }
                user = new User { Role = Roles.Agent, Agent = agent };
                db.Users.Add(user);
            }
            user.UserName = login;
            user.Email = EmailBox.Text.Trim();
            if (PasswordBox.Text != "") user.Password = PasswordHasher.Hash(PasswordBox.Text);
            if (user.Lock && !LockBox.Checked) user.FailedLoginCount = 0; // unlocking resets the counter
            user.Lock = LockBox.Checked;
        }
        db.SaveChanges();
        Reload();
    }

    private void Delete()
    {
        if (_id is null) return;
        using var db = Program.Db();
        if (db.Orders.Any(o => o.AgentID == _id)) { Ui.Error(this, "This agent has orders and cannot be deleted. Lock its account instead."); return; }
        if (MessageBox.Show(this, $"Delete agent '{NameBox.Text}' and its portal account?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        db.Agents.Where(a => a.AgentID == _id).ExecuteDelete(); // Users row removed by ON DELETE CASCADE
        Clear();
        Reload();
    }
}
