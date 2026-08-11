using System.Security.Cryptography;
using System.Text;

namespace PersonalDigitalVault.Api.Services;

public interface IEncryptionService
{
    byte[] EncryptBytes(byte[] plainBytes);
    byte[] DecryptBytes(byte[] encryptedBytes);
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
}

public sealed class EncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:KeyBase64"];
        if (string.IsNullOrWhiteSpace(keyBase64))
            throw new InvalidOperationException("Encryption:KeyBase64 is not configured.");

        try
        {
            _key = Convert.FromBase64String(keyBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Encryption key must be valid Base64.", ex);
        }

        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption key must decode to exactly 32 bytes for AES-256.");
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var cipher = new byte[plainBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var output = new byte[NonceSize + TagSize + cipher.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, output, NonceSize, TagSize);
        Buffer.BlockCopy(cipher, 0, output, NonceSize + TagSize, cipher.Length);
        return output;
    }

    public byte[] DecryptBytes(byte[] encryptedBytes)
    {
        if (encryptedBytes.Length < NonceSize + TagSize)
            throw new CryptographicException("Encrypted payload is invalid.");

        var nonce = encryptedBytes[..NonceSize];
        var tag = encryptedBytes[NonceSize..(NonceSize + TagSize)];
        var cipher = encryptedBytes[(NonceSize + TagSize)..];
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);
        return plain;
    }

    public string EncryptString(string plainText) =>
        Convert.ToBase64String(EncryptBytes(Encoding.UTF8.GetBytes(plainText)));

    public string DecryptString(string cipherText) =>
        Encoding.UTF8.GetString(DecryptBytes(Convert.FromBase64String(cipherText)));
}
