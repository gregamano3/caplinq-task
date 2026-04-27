using System.Security.Cryptography;
using System.Text;
using CarrierRates.Application.Abstractions.Auth;

namespace CarrierRates.Infrastructure.Auth;

public class Sha256PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password using SHA256 for assessment simplicity.
    /// </summary>
    public string Hash(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Verifies a plaintext password against a SHA256 hash value.
    /// </summary>
    public bool Verify(string plainText, string hash)
    {
        return string.Equals(Hash(plainText), hash, StringComparison.OrdinalIgnoreCase);
    }
}
