using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.DTOs;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    int CredentialCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateCategoryRequest([Required, MaxLength(100)] string Name);
public sealed record RenameCategoryRequest([Required, MaxLength(100)] string Name);
