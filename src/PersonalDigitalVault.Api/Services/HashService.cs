using System.Security.Cryptography;

namespace PersonalDigitalVault.Api.Services;

public interface IHashService
{
    string ComputeSha256(byte[] data);
}

public sealed class HashService : IHashService
{
    public string ComputeSha256(byte[] data) =>
        Convert.ToHexString(SHA256.HashData(data));
}
