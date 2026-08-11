using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.DTOs;

public sealed record AdminUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record UpdateUserStatusRequest(bool IsActive);

public sealed record DashboardResponse(
    int TotalUsers,
    long TotalUploads,
    int TotalStoredFiles,
    int TotalCredentialRecords,
    DateTime GeneratedAtUtc);

public sealed record UpdateSettingRequest([Required, MaxLength(1000)] string Value);
