using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.DTOs;

public sealed record ProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    DateTime CreatedAtUtc);

public sealed record UpdateProfileRequest(
    [Required, MaxLength(120)] string FullName,
    [MaxLength(30)] string? PhoneNumber);
