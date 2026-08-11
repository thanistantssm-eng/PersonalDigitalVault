namespace PersonalDigitalVault.Api.Services;

public interface IFileStorageService
{
    Task<(string StoredFileName, string RelativePath)> SaveAsync(Guid userId, byte[] encryptedData, CancellationToken cancellationToken);
    Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken);
}

public sealed class FileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public FileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var configured = configuration["Storage:RootPath"] ?? "ProtectedStorage";
        _rootPath = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string StoredFileName, string RelativePath)> SaveAsync(
        Guid userId,
        byte[] encryptedData,
        CancellationToken cancellationToken)
    {
        var userDirectory = Path.Combine(_rootPath, userId.ToString("N"));
        Directory.CreateDirectory(userDirectory);

        var storedFileName = $"{Guid.NewGuid():N}.vault";
        var fullPath = Path.Combine(userDirectory, storedFileName);
        await File.WriteAllBytesAsync(fullPath, encryptedData, cancellationToken);

        var relativePath = Path.Combine(userId.ToString("N"), storedFileName);
        return (storedFileName, relativePath);
    }

    public Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken) =>
        File.ReadAllBytesAsync(GetSafePath(relativePath), cancellationToken);

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken)
    {
        var fullPath = GetSafePath(relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    private string GetSafePath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        var rootWithSeparator = Path.GetFullPath(_rootPath).TrimEnd(Path.DirectorySeparatorChar)
                                + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");

        return fullPath;
    }
}
