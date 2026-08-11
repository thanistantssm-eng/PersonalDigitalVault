using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.DTOs;

public sealed record RegisterRequest(
    [Required, MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [MaxLength(30)] string? PhoneNumber);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string FullName,
    string Email,
    string Role);
