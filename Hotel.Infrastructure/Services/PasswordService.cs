using System.Security.Cryptography;
using Hotel.Application.Interfaces;

namespace Hotel.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const string Prefix = "PBKDF2";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        var key = pbkdf2.GetBytes(KeySize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string storedValue, string password)
    {
        if (string.IsNullOrWhiteSpace(storedValue) || string.IsNullOrWhiteSpace(password))
            return false;

        if (!IsHashed(storedValue))
            return storedValue == password;

        var parts = storedValue.Split('$');
        if (parts.Length != 4)
            return false;

        if (!int.TryParse(parts[1], out var iterations))
            return false;

        byte[] salt;
        byte[] expectedKey;

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedKey = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256);

        var actualKey = pbkdf2.GetBytes(expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    public bool IsHashed(string storedValue)
    {
        return !string.IsNullOrWhiteSpace(storedValue)
               && storedValue.StartsWith($"{Prefix}$", StringComparison.Ordinal);
    }
}