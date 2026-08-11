using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalDigitalVault.Api.Data;
using PersonalDigitalVault.Api.DTOs;
using PersonalDigitalVault.Api.Extensions;
using PersonalDigitalVault.Api.Models;

namespace PersonalDigitalVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CategoriesController(VaultDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var categories = await db.Categories.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var credentialCategories = await db.CredentialRecords.AsNoTracking()
            .Where(x => x.UserId == userId && x.Category != null)
            .Select(x => x.Category!)
            .ToListAsync(cancellationToken);

        var counts = credentialCategories
            .Select(x => x.Trim())
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Count(), StringComparer.OrdinalIgnoreCase);

        var response = categories.Select(x =>
        {
            var count = counts.TryGetValue(x.Name, out var value) ? value : 0;
            return new CategoryResponse(x.Id, x.Name, count, x.CreatedAtUtc, x.UpdatedAtUtc);
        }).ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var category = await db.Categories.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (category is null) return NotFound();

        var count = await db.CredentialRecords.AsNoTracking()
            .CountAsync(x => x.UserId == userId && x.Category == category.Name, cancellationToken);

        return Ok(new CategoryResponse(category.Id, category.Name, count,
            category.CreatedAtUtc, category.UpdatedAtUtc));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var name = NormalizeName(request.Name);
        if (name is null) return BadRequest(new { message = "A category name is required." });

        if (await db.Categories.AnyAsync(x => x.UserId == userId && x.Name == name, cancellationToken))
            return Conflict(new { message = "A category with this name already exists." });

        var category = new VaultCategory { Name = name, UserId = userId };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            new CategoryResponse(category.Id, category.Name, 0, category.CreatedAtUtc, category.UpdatedAtUtc));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> Rename(Guid id, RenameCategoryRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (category is null) return NotFound();

        var newName = NormalizeName(request.Name);
        if (newName is null) return BadRequest(new { message = "A category name is required." });

        if (await db.Categories.AnyAsync(x => x.UserId == userId && x.Id != id && x.Name == newName, cancellationToken))
            return Conflict(new { message = "A category with this name already exists." });

        var oldName = category.Name;
        var linkedCredentials = await db.CredentialRecords
            .Where(x => x.UserId == userId && x.Category == oldName)
            .ToListAsync(cancellationToken);

        foreach (var credential in linkedCredentials)
        {
            credential.Category = newName;
            credential.UpdatedAtUtc = DateTime.UtcNow;
        }

        category.Name = newName;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new CategoryResponse(category.Id, category.Name, linkedCredentials.Count,
            category.CreatedAtUtc, category.UpdatedAtUtc));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (category is null) return NotFound();

        var linkedCredentials = await db.CredentialRecords
            .Where(x => x.UserId == userId && x.Category == category.Name)
            .ToListAsync(cancellationToken);

        foreach (var credential in linkedCredentials)
        {
            credential.Category = null;
            credential.UpdatedAtUtc = DateTime.UtcNow;
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}
