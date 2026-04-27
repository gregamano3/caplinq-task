using System.Security.Cryptography;
using System.Text;
using CarrierRates.Application.Abstractions.Auth;

namespace CarrierRates.Infrastructure.Auth;

public class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public bool Verify(string plainText, string hash)
    {
        return string.Equals(Hash(plainText), hash, StringComparison.OrdinalIgnoreCase);
    }
}
