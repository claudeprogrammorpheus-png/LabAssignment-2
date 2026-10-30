using System.Configuration;
using B2B.Data;
using B2B.WinForms.Forms;
using Microsoft.EntityFrameworkCore;

namespace B2B.WinForms;

internal static class Program
{
    /// <summary>Creates a fresh DbContext per operation so every screen sees the latest web orders.</summary>
    public static Func<B2BContext> Db { get; private set; } = null!;

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // Connection string is read from App.config (Task: secure configuration).
        var cs = ConfigurationManager.ConnectionStrings["B2BDb"]?.ConnectionString;
        if (string.IsNullOrWhiteSpace(cs))
        {
            MessageBox.Show("Connection string 'B2BDb' is missing in App.config.", "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        var options = new DbContextOptionsBuilder<B2BContext>().UseSqlServer(cs).Options;
        Db = () => new B2BContext(options);

        try
        {
            using var db = Db();
            db.Database.Migrate(); // same Code First schema as the web portal
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot connect to the database. Check App.config.\n\n" + ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (args.Length >= 2 && args[0] == "--screenshots")
        {
            Tour.Run(args[1]);
            return;
        }

        using var login = new LoginForm();
        if (login.ShowDialog() != DialogResult.OK) return;
        Application.Run(new MainForm(login.LoggedInUser!));
    }
}
