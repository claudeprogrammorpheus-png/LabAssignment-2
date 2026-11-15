using System.Security.Cryptography;

namespace B2B.Core;

/// <summary>Order money calculations shared by the web portal, WinForms and reports.</summary>
public static class OrderCalculator
{
    public static decimal Total(IEnumerable<OrderDetail> lines) => lines.Sum(l => l.Quantity * l.UnitAmount);

    public static decimal Total(IEnumerable<(int quantity, decimal unitAmount)> lines) => lines.Sum(l => l.quantity * l.unitAmount);
}

/// <summary>PBKDF2-SHA256 password hashing. Stored format: iterations.salt.hash (Base64).</summary>
public static class PasswordHasher
{
    private const int Iterations = 100_000, SaltSize = 16, KeySize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var it)) return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, it, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

public enum LoginResult { Success, InvalidCredentials, Locked, WrongRole }

/// <summary>Account lockout policy: lock the account after <see cref="MaxFailedAttempts"/> wrong passwords in a row.</summary>
public static class LockoutPolicy
{
    public const int MaxFailedAttempts = 5;

    public static LoginResult Evaluate(User? user, string password, string requiredRole)
    {
        if (user is null) return LoginResult.InvalidCredentials;
        if (user.Lock) return LoginResult.Locked;
        if (!PasswordHasher.Verify(password, user.Password))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedAttempts) user.Lock = true;
            return user.Lock ? LoginResult.Locked : LoginResult.InvalidCredentials;
        }
        user.FailedLoginCount = 0;
        return user.Role == requiredRole ? LoginResult.Success : LoginResult.WrongRole;
    }

    public static string Message(LoginResult r) => r switch
    {
        LoginResult.Success => "Login successful.",
        LoginResult.Locked => "This account is locked. Please contact the administrator.",
        LoginResult.WrongRole => "You are not allowed to use this application with this account.",
        _ => "Invalid user name or password.",
    };
}

/// <summary>Allowed order status changes (Admin order management).</summary>
public static class OrderWorkflow
{
    private static readonly Dictionary<string, string[]> Next = new()
    {
        [OrderStatuses.Pending] = [OrderStatuses.Processing, OrderStatuses.Cancelled],
        [OrderStatuses.Processing] = [OrderStatuses.Shipped, OrderStatuses.Cancelled],
        [OrderStatuses.Shipped] = [OrderStatuses.Delivered],
        [OrderStatuses.Delivered] = [],
        [OrderStatuses.Cancelled] = [],
    };

    public static IReadOnlyList<string> AllowedNext(string current) => Next.TryGetValue(current, out var n) ? n : [];

    public static bool CanChange(string from, string to) => AllowedNext(from).Contains(to);
}

/// <summary>Thrown when an order cannot be placed (insufficient stock, invalid lines...).</summary>
public class OrderException(string message) : Exception(message);
