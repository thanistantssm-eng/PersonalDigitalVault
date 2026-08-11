using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.Models;

public sealed class VaultUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [MaxLength(30)]
    public string Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<VaultFolder> Folders { get; set; } = new List<VaultFolder>();
    public ICollection<VaultCategory> Categories { get; set; } = new List<VaultCategory>();
    public ICollection<StoredDocument> Documents { get; set; } = new List<StoredDocument>();
    public ICollection<CredentialRecord> CredentialRecords { get; set; } = new List<CredentialRecord>();
}
