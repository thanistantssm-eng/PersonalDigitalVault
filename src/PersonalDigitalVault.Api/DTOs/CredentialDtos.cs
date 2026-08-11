using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.DTOs;

public sealed record CredentialSummaryResponse(
    Guid Id,
    string Title,
    string? Category,
    string Username,
    string Secret,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CredentialRevealResponse(
    Guid Id,
    string Title,
    string? Category,
    string Username,
    string Secret,
    string? Website,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateCredentialRequest(
    [Required, MaxLength(150)] string Title,
    [MaxLength(100)] string? Category,
    [Required, MaxLength(500)] string Username,
    [Required, MaxLength(1000)] string Secret,
    [MaxLength(500)] string? Website,
    [MaxLength(2000)] string? Notes);

public sealed record UpdateCredentialRequest(
    [Required, MaxLength(150)] string Title,
    [MaxLength(100)] string? Category,
    [Required, MaxLength(500)] string Username,
    [Required, MaxLength(1000)] string Secret,
    [MaxLength(500)] string? Website,
    [MaxLength(2000)] string? Notes);
