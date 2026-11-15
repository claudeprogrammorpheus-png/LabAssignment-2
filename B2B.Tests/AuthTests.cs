using B2B.Core;
using B2B.Data;
using Microsoft.EntityFrameworkCore;

namespace B2B.Tests;

public class AuthTests : IDisposable
{
    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    private (LoginResult, User?) Login(string user, string pass, string role)
    {
        using var db = _db.New();
        return new AuthService(db).Login(user, pass, role);
    }

    private User Get(string userName)
    {
        using var db = _db.New();
        return db.Users.AsNoTracking().Single(u => u.UserName == userName);
    }

    [Fact]
    public void Agent_can_log_in_to_portal_by_user_name_or_email()
    {
        Assert.Equal(LoginResult.Success, Login("agent01", "Agent@123", Roles.Agent).Item1);
        var (r, u) = Login("agent02@b2bwholesale.vn", "Agent@123", Roles.Agent);
        Assert.Equal(LoginResult.Success, r);
        Assert.Equal(2, u!.AgentID);
    }

    [Fact]
    public void Role_is_enforced_for_each_application()
    {
        Assert.Equal(LoginResult.WrongRole, Login("agent01", "Agent@123", Roles.Admin).Item1); // agent -> WinForms
        Assert.Equal(LoginResult.WrongRole, Login("admin", "Admin@123", Roles.Agent).Item1);   // admin -> web portal
        Assert.Equal(LoginResult.Success, Login("admin", "Admin@123", Roles.Admin).Item1);
    }

    [Fact]
    public void Account_is_locked_after_five_wrong_passwords()
    {
        for (int i = 1; i < LockoutPolicy.MaxFailedAttempts; i++)
        {
            Assert.Equal(LoginResult.InvalidCredentials, Login("agent03", "wrong", Roles.Agent).Item1);
            Assert.False(Get("agent03").Lock);
        }
        Assert.Equal(LoginResult.Locked, Login("agent03", "wrong", Roles.Agent).Item1);
        Assert.True(Get("agent03").Lock); // persisted in the Users table
        // Even the correct password is refused once locked.
        Assert.Equal(LoginResult.Locked, Login("agent03", "Agent@123", Roles.Agent).Item1);
    }

    [Fact]
    public void Seeded_locked_account_cannot_log_in()
    {
        Assert.Equal(LoginResult.Locked, Login("agent20", "Agent@123", Roles.Agent).Item1);
    }

    [Fact]
    public void Successful_login_resets_failed_counter()
    {
        Login("agent04", "wrong", Roles.Agent);
        Login("agent04", "wrong", Roles.Agent);
        Assert.Equal(2, Get("agent04").FailedLoginCount);
        Assert.Equal(LoginResult.Success, Login("agent04", "Agent@123", Roles.Agent).Item1);
        Assert.Equal(0, Get("agent04").FailedLoginCount);
    }

    [Fact]
    public void Unknown_user_gets_generic_error()
    {
        var (r, u) = Login("nobody", "x", Roles.Agent);
        Assert.Equal(LoginResult.InvalidCredentials, r);
        Assert.Null(u);
    }

    [Fact]
    public void Passwords_are_salted_hashes_never_plain_text()
    {
        var h1 = PasswordHasher.Hash("Secret#1");
        var h2 = PasswordHasher.Hash("Secret#1");
        Assert.NotEqual(h1, h2);                       // random salt
        Assert.DoesNotContain("Secret#1", h1);
        Assert.True(PasswordHasher.Verify("Secret#1", h1));
        Assert.False(PasswordHasher.Verify("secret#1", h1));
        Assert.True(PasswordHasher.Verify("Admin@123", SeedData.AdminHash));
        Assert.True(PasswordHasher.Verify("Agent@123", SeedData.AgentHash));
    }

    [Fact]
    public void Seed_data_has_15_to_30_rows_per_table()
    {
        using var db = _db.New();
        foreach (var count in new[] { db.Items.Count(), db.Agents.Count(), db.Orders.Count(), db.OrderDetails.Count(), db.Users.Count() })
            Assert.InRange(count, 15, 30);
    }
}
