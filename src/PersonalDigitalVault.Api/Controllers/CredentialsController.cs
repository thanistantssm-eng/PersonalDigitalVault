using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalDigitalVault.Api.Data;
using PersonalDigitalVault.Api.DTOs;
using PersonalDigitalVault.Api.Extensions;
using PersonalDigitalVault.Api.Models;
using PersonalDigitalVault.Api.Services;

namespace PersonalDigitalVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CredentialsController(VaultDbContext db, IEncryptionService encryption) : ControllerBase
{
    private const string Mask = "••••••••";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CredentialSummaryResponse>>> GetAll(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var query = db.CredentialRecords.AsNoTracking().Where(x => x.UserId == userId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x => x.Title.Contains(value) || (x.Category != null && x.Category.Contains(value)));
        }

        var items = await query.OrderBy(x => x.Title)
            .Select(x => new CredentialSummaryResponse(x.Id, x.Title, x.Category, Mask, Mask, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CredentialSummaryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var item = await db.CredentialRecords.AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new CredentialSummaryResponse(x.Id, x.Title, x.Category, Mask, Mask, x.CreatedAtUtc, x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:guid}/reveal")]
    public async Task<ActionResult<CredentialRevealResponse>> Reveal(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var item = await db.CredentialRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (item is null) return NotFound();

        return Ok(new CredentialRevealResponse(
            item.Id, item.Title, item.Category,
            encryption.DecryptString(item.UsernameCipherText),
            encryption.DecryptString(item.SecretCipherText),
            DecryptOptional(item.WebsiteCipherText),
            DecryptOptional(item.NotesCipherText),
            item.CreatedAtUtc, item.UpdatedAtUtc));
    }

    [HttpPost]
    public async Task<ActionResult<CredentialSummaryResponse>> Create(CreateCredentialRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var category = await EnsureCategoryAsync(userId, CleanOptional(request.Category), cancellationToken);

        var item = new CredentialRecord
        {
            Title = request.Title.Trim(),
            Category = category,
            UsernameCipherText = encryption.EncryptString(request.Username),
            SecretCipherText = encryption.EncryptString(request.Secret),
            WebsiteCipherText = EncryptOptional(request.Website),
            NotesCipherText = EncryptOptional(request.Notes),
            UserId = userId
        };
        db.CredentialRecords.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            new CredentialSummaryResponse(item.Id, item.Title, item.Category, Mask, Mask, item.CreatedAtUtc, item.UpdatedAtUtc));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CredentialSummaryResponse>> Update(
        Guid id,
        UpdateCredentialRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var item = await db.CredentialRecords.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (item is null) return NotFound();

        var category = await EnsureCategoryAsync(userId, CleanOptional(request.Category), cancellationToken);
        item.Title = request.Title.Trim();
        item.Category = category;
        item.UsernameCipherText = encryption.EncryptString(request.Username);
        item.SecretCipherText = encryption.EncryptString(request.Secret);
        item.WebsiteCipherText = EncryptOptional(request.Website);
        item.NotesCipherText = EncryptOptional(request.Notes);
        item.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new CredentialSummaryResponse(item.Id, item.Title, item.Category, Mask, Mask, item.CreatedAtUtc, item.UpdatedAtUtc));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var item = await db.CredentialRecords.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (item is null) return NotFound();
        db.CredentialRecords.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<string?> EnsureCategoryAsync(Guid userId, string? categoryName, CancellationToken cancellationToken)
    {
        if (categoryName is null) return null;

        var existing = await db.Categories.SingleOrDefaultAsync(
            x => x.UserId == userId && x.Name == categoryName,
            cancellationToken);

        if (existing is not null) return existing.Name;

        db.Categories.Add(new VaultCategory
        {
            UserId = userId,
            Name = categoryName
        });
        return categoryName;
    }

    private string? EncryptOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : encryption.EncryptString(value.Trim());
    private string? DecryptOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : encryption.DecryptString(value);
    private static string? CleanOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
