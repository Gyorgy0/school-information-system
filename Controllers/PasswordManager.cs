using System.Security.Cryptography;
using System.Text;

public class PasswordManager
{
    private static Random random = new Random();

    public static string Hash(string value)
    {
        SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
        StringBuilder builder = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

    public static string GenerateSalt()
    {
        StringBuilder builder = new();
        for (int q = 0; q < 32; q++)
        {
            builder.Append(random.Next(16).ToString("x"));
        }

        return builder.ToString();
    }

    public static string GeneratePasswordHash(string Password, string Salt)
    {
        return Hash(Password + Salt);
    }

    public static bool Verify(string CandidatePassword, string PasswordSalt, string PasswordHash)
    {
        var CandidateHash = GeneratePasswordHash(CandidatePassword, PasswordSalt);
        return CandidateHash == PasswordHash;
    }
}
