using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Scholarship.Api.Security;

public interface IPasswordHasher
{
    byte[] HashPassword(string password);
    bool VerifyPassword(string password, byte[] storedHash);
}

public class Argon2idPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 3;
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 4;

    public byte[] HashPassword(string password)
    {
        byte[] salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };

        byte[] hash = argon2.GetBytes(HashSize);

        // Format: [16 bytes Salt] + [32 bytes Hash] = 48 bytes (fits easily in varbinary(255))
        byte[] result = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, HashSize);

        return result;
    }

    public bool VerifyPassword(string password, byte[] storedHash)
    {
        if (storedHash == null || storedHash.Length < (SaltSize + HashSize))
            return false;

        byte[] salt = new byte[SaltSize];
        byte[] originalHash = new byte[HashSize];

        Buffer.BlockCopy(storedHash, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(storedHash, SaltSize, originalHash, 0, HashSize);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };

        byte[] computedHash = argon2.GetBytes(HashSize);
        return CryptographicOperations.FixedTimeEquals(originalHash, computedHash);
    }
}
