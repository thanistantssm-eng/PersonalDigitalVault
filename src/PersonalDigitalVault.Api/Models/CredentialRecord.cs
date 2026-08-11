using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.Models;

public sealed class CredentialRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public string UsernameCipherText { get; set; } = string.Empty;
    public string SecretCipherText { get; set; } = string.Empty;
    public string? WebsiteCipherText { get; set; }
    public string? NotesCipherText { get; set; }

    public Guid UserId { get; set; }
    public VaultUser User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
