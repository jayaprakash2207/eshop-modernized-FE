using System.Security.Cryptography;
using System.Text;

namespace PlatformApp.Infrastructure.Identity;

/// <summary>
/// Simple password hashing utility using PBKDF2 (Password-Based Key Derivation Function 2).
/// Production systems should use BCrypt.Net or Argon2 for higher security.
/// </summary>
public sealed class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 10000;
    private const char SegmentDelimiter = '$';

    /// <summary>
    /// Hashes a password with a random salt using PBKDF2-SHA256.
    /// Format: $PBKDF2$iterations$salt$hash
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        var hash = pbkdf2.GetBytes(HashSize);
        var saltBase64 = Convert.ToBase64String(salt);
        var hashBase64 = Convert.ToBase64String(hash);

        return $"PBKDF2{SegmentDelimiter}{Iterations}{SegmentDelimiter}{saltBase64}{SegmentDelimiter}{hashBase64}";
    }

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            var parts = hash.Split(SegmentDelimiter);
            if (parts.Length != 4 || !parts[0].Equals("PBKDF2", StringComparison.Ordinal))
            {
                return false;
            }

            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var storedHash = Convert.FromBase64String(parts[3]);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            var computedHash = pbkdf2.GetBytes(HashSize);
            return ConstantTimeEquals(storedHash, computedHash);
        }
        catch
        {
            // If hash format is invalid or parsing fails, return false
            return false;
        }
    }

    /// <summary>
    /// Constant-time comparison to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
