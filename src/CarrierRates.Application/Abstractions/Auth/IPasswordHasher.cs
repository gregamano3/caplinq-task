namespace CarrierRates.Application.Abstractions.Auth;

public interface IPasswordHasher
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
}
