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
public sealed class FoldersController(VaultDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FolderResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var items = await db.Folders.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(x => new FolderResponse(x.Id, x.Name, x.Documents.Count, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FolderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var item = await db.Folders.AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new FolderResponse(x.Id, x.Name, x.Documents.Count, x.CreatedAtUtc, x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<FolderResponse>> Create(CreateFolderRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var name = request.Name.Trim();
        if (await db.Folders.AnyAsync(x => x.UserId == userId && x.Name == name, cancellationToken))
            return Conflict(new { message = "A folder with this name already exists." });

        var folder = new VaultFolder { Name = name, UserId = userId };
        db.Folders.Add(folder);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = folder.Id },
            new FolderResponse(folder.Id, folder.Name, 0, folder.CreatedAtUtc, folder.UpdatedAtUtc));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FolderResponse>> Rename(Guid id, RenameFolderRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var folder = await db.Folders.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (folder is null) return NotFound();

        var name = request.Name.Trim();
        if (await db.Folders.AnyAsync(x => x.UserId == userId && x.Name == name && x.Id != id, cancellationToken))
            return Conflict(new { message = "A folder with this name already exists." });

        folder.Name = name;
        folder.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        var count = await db.Documents.CountAsync(x => x.FolderId == id, cancellationToken);
        return Ok(new FolderResponse(folder.Id, folder.Name, count, folder.CreatedAtUtc, folder.UpdatedAtUtc));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var folder = await db.Folders.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (folder is null) return NotFound();

        var documents = await db.Documents.Where(x => x.FolderId == id && x.UserId == userId).ToListAsync(cancellationToken);
        foreach (var document in documents) document.FolderId = null;
        db.Folders.Remove(folder);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
