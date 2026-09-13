using System.Security.Cryptography;
using System.Text;

namespace Scholarship.Api.Security;

public interface ICryptoService
{
    byte[] Encrypt(string plainText);
    string Decrypt(byte[] cipherData);
}

public interface IHmacBlindHasher
{
    byte[] ComputeHash(string input);
}

public class AesGcmCryptoService : ICryptoService
{
    private readonly byte[] _key;

    public AesGcmCryptoService(IConfiguration configuration)
    {
        var keyHex = configuration["Security:EncryptionKey"] 
            ?? "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"; // 256-bit default fallback
        _key = Convert.FromHexString(keyHex);
    }

    public byte[] Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return Array.Empty<byte>();

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aesGcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Format: [12 bytes Nonce] + [16 bytes Tag] + [CipherBytes]
        byte[] result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return result;
    }

    public string Decrypt(byte[] cipherData)
    {
        if (cipherData == null || cipherData.Length < 28) // Nonce(12) + Tag(16) = 28
            return string.Empty;

        int nonceSize = AesGcm.NonceByteSizes.MaxSize;
        int tagSize = AesGcm.TagByteSizes.MaxSize;
        int cipherSize = cipherData.Length - nonceSize - tagSize;

        byte[] nonce = new byte[nonceSize];
        byte[] tag = new byte[tagSize];
        byte[] cipherBytes = new byte[cipherSize];
        byte[] plainBytes = new byte[cipherSize];

        Buffer.BlockCopy(cipherData, 0, nonce, 0, nonceSize);
        Buffer.BlockCopy(cipherData, nonceSize, tag, 0, tagSize);
        Buffer.BlockCopy(cipherData, nonceSize + tagSize, cipherBytes, 0, cipherSize);

        using var aesGcm = new AesGcm(_key, tagSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}

public class HmacSha256BlindHasher : IHmacBlindHasher
{
    private readonly byte[] _pepperKey;

    public HmacSha256BlindHasher(IConfiguration configuration)
    {
        var pepper = configuration["Security:BlindIndexPepper"] 
            ?? "ScholarshipSystemPepperKey2026_SecureHashingPepper!";
        _pepperKey = Encoding.UTF8.GetBytes(pepper);
    }

    public byte[] ComputeHash(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new byte[32];

        // Normalize: trim, lowercase for deterministic blind index lookups
        string normalized = input.Trim().ToLowerInvariant();
        using var hmac = new HMACSHA256(_pepperKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(normalized));
    }
}
