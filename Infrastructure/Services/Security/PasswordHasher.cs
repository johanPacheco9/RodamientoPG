using System.Security.Cryptography;

namespace Infrastructure.Services.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 10000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA1,
            HashSize);

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool Verify(string password, string dbPasswordBase64)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(dbPasswordBase64);
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var userHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA1,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(hashBytes.AsSpan(SaltSize, HashSize), userHash);
        }
        catch
        {
            return false;
        }
    }
}
