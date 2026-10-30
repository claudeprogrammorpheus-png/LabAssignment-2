using B2B.Core;
using B2B.Data;
using QuestPDF.Fluent;

namespace B2B.WinForms.Forms;

/// <summary>
/// Automated walkthrough (B2B.WinForms.exe --screenshots &lt;folder&gt;): drives the real forms and saves screenshots
/// for the report. Useful as a smoke test after each merge.
/// </summary>
internal static class Tour
{
    public static void Run(string dir)
    {
        Directory.CreateDirectory(dir);
        int n = 0;
        void Snap(Form f, string name)
        {
            for (int i = 0; i < 6; i++) Application.DoEvents();
            using var bmp = new Bitmap(f.Width, f.Height);
            f.DrawToBitmap(bmp, new Rectangle(0, 0, f.Width, f.Height));
            bmp.Save(Path.Combine(dir, $"{++n:00}_{name}.png"));
        }

        // Login: agent account is rejected, admin accepted.
        var login = new LoginForm();
        login.Show();
        login.UserBox.Text = "agent01"; login.PassBox.Text = "Agent@123";
        login.DoLogin();
        Snap(login, "winforms_login_agent_rejected");
        login.UserBox.Text = "admin"; login.PassBox.Text = "Admin@123";
        Snap(login, "winforms_login");
        login.DoLogin();
        var admin = login.LoggedInUser ?? throw new InvalidOperationException("Admin login failed");

        var main = new MainForm(admin);
        main.Show();
        main.Items.Reload();
        main.Items.Select(15);
        Snap(main, "winforms_items_crud");

        main.Tabs.SelectedIndex = 1;
        main.Agents.Reload();
        Snap(main, "winforms_agents_crud");

        main.Tabs.SelectedIndex = 2;
        main.Orders.Reload();
        int newest;
        using (var db = Program.Db()) newest = db.Orders.Max(o => o.OrderID);
        main.Orders.SelectOrder(newest);
        Snap(main, "winforms_orders_incoming");
        main.Orders.NextStatus.SelectedItem = OrderStatuses.Processing;
        main.Orders.ApplyStatus();
        Snap(main, "winforms_orders_status_updated");

        var pdf = main.Orders.PrintManifest(open: false)!;
        File.Copy(pdf, Path.Combine(dir, Path.GetFileName(pdf)), true);
        using (var db = Program.Db())
        {
            var order = new OrderService(db).GetOrder(newest)!;
            var img = ShippingManifest.Build(order).GenerateImages(new QuestPDF.Infrastructure.ImageGenerationSettings { RasterDpi = 110 }).First();
            File.WriteAllBytes(Path.Combine(dir, $"{++n:00}_winforms_shipping_manifest.png"), img);
        }

        main.Tabs.SelectedIndex = 3;
        main.Dashboard.Reload();
        Snap(main, "winforms_dashboard");
        main.Close();
    }
}
