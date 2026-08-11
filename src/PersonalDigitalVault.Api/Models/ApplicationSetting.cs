using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.Models;

public sealed class ApplicationSetting
{
    [Key, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Value { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
