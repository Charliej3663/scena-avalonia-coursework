using System.Security.Cryptography;
using System.Text.Json;

namespace ScenaAvalonia;

internal static class AdminCredentials
{
    public const string DefaultUserName = "admin";
    private const int Iterations = 210_000;
    private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScenaStudio", "admin-credentials.json");
    public static bool IsConfigured => File.Exists(FilePath);

    public static bool Verify(string userName, string password)
    {
        if (!IsConfigured || !string.Equals(userName.Trim(), DefaultUserName, StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            var credential = JsonSerializer.Deserialize<Credential>(File.ReadAllText(FilePath));
            if (credential is null) return false;
            var expected = Convert.FromBase64String(credential.Hash);
            var actual = Hash(password, Convert.FromBase64String(credential.Salt));
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch { return false; }
    }

    public static bool ChangePassword(string currentPassword, string newPassword)
    {
        if (!Verify(DefaultUserName, currentPassword) || newPassword.Length < 10) return false;
        var salt = RandomNumberGenerator.GetBytes(16);
        var credential = new Credential(Convert.ToBase64String(salt), Convert.ToBase64String(Hash(newPassword, salt)));
        var directory = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(credential));
        return true;
    }

    public static bool CreateInitialPassword(string password)
    {
        if (IsConfigured || password.Length < 10) return false;
        var salt = RandomNumberGenerator.GetBytes(16);
        var initial = new Credential(Convert.ToBase64String(salt), Convert.ToBase64String(Hash(password, salt)));
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(initial));
        return true;
    }

    private static byte[] Hash(string password, byte[] salt) => Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
    private sealed record Credential(string Salt, string Hash);
}

