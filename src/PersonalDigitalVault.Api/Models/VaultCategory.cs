using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.Models;

public sealed class VaultCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public VaultUser User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
