using B2B.Core;
using B2B.Data;

namespace B2B.WinForms.Forms;

/// <summary>Task 1.1 - secure login: only Admin accounts may open the back-office application.</summary>
public class LoginForm : Form
{
    public readonly TextBox UserBox = new() { Width = 300, Font = Ui.F(11), PlaceholderText = "User name or e-mail" };
    public readonly TextBox PassBox = new() { Width = 300, Font = Ui.F(11), UseSystemPasswordChar = true, PlaceholderText = "Password" };
    public readonly Label Message = new() { AutoSize = false, Width = 300, Height = 40, ForeColor = Ui.Red, Font = Ui.F(9) };
    public User? LoggedInUser { get; private set; }

    public LoginForm()
    {
        Text = "B2B Wholesale - Back-office login";
        ClientSize = new Size(420, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        var header = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Ui.Navy };
        header.Controls.Add(new Label { Text = "B2B Wholesale", Font = Ui.F(18, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(28, 24) });
        header.Controls.Add(new Label { Text = "Warehouse back-office · Admin only", Font = Ui.F(10), ForeColor = Color.FromArgb(0xDD, 0xE8, 0xF3), AutoSize = true, Location = new Point(30, 66) });

        var login = Ui.Btn("Sign in", Ui.Blue, 300);
        login.Height = 40;
        login.Click += (_, _) => DoLogin();
        AcceptButton = login;

        var body = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(58, 24, 0, 0), WrapContents = false };
        body.Controls.Add(Ui.Lbl("User name or e-mail", 9.5f, FontStyle.Bold));
        body.Controls.Add(UserBox);
        body.Controls.Add(Ui.Lbl("Password", 9.5f, FontStyle.Bold));
        body.Controls.Add(PassBox);
        body.Controls.Add(new Panel { Height = 10 });
        body.Controls.Add(login);
        body.Controls.Add(Message);

        Controls.Add(body);
        Controls.Add(header);
    }

    public void DoLogin()
    {
        using var db = Program.Db();
        var (result, user) = new AuthService(db).Login(UserBox.Text, PassBox.Text, Roles.Admin);
        if (result == LoginResult.Success)
        {
            LoggedInUser = user;
            DialogResult = DialogResult.OK;
            Close();
            return;
        }
        Message.Text = LockoutPolicy.Message(result);
        PassBox.Clear();
        PassBox.Focus();
    }
}
