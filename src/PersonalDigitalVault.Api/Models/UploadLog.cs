namespace PersonalDigitalVault.Api.Models;

public sealed class UploadLog
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DocumentId { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
