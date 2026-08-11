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
public sealed class DocumentsController(
    VaultDbContext db,
    IEncryptionService encryption,
    IHashService hashService,
    IFileStorageService storage,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? folderId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var query = db.Documents.AsNoTracking().Where(x => x.UserId == userId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.OriginalFileName.Contains(search.Trim()));
        if (folderId.HasValue) query = query.Where(x => x.FolderId == folderId.Value);

        var items = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new DocumentResponse(
                x.Id, x.OriginalFileName, x.ContentType, x.FileSizeBytes, x.IntegrityHashSha256,
                x.FolderId, x.Folder == null ? null : x.Folder.Name, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var item = await db.Documents.AsNoTracking()
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new DocumentResponse(
                x.Id, x.OriginalFileName, x.ContentType, x.FileSizeBytes, x.IntegrityHashSha256,
                x.FolderId, x.Folder == null ? null : x.Folder.Name, x.CreatedAtUtc, x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<DocumentResponse>> Upload([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var file = request.File;
        if (file.Length <= 0) return BadRequest(new { message = "The uploaded file is empty." });

        var maxSize = configuration.GetValue<long?>("Storage:MaxFileSizeBytes") ?? 10_485_760;
        if (file.Length > maxSize) return BadRequest(new { message = $"File size exceeds the {maxSize} byte limit." });

        var originalName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        var allowed = configuration.GetSection("Storage:AllowedExtensions").Get<string[]>() ?? [];
        if (allowed.Length > 0 && !allowed.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "This file type is not allowed." });

        if (request.FolderId.HasValue && !await db.Folders.AnyAsync(
                x => x.Id == request.FolderId.Value && x.UserId == userId, cancellationToken))
            return BadRequest(new { message = "The selected folder does not belong to this user." });

        await using var memory = new MemoryStream();
        await file.CopyToAsync(memory, cancellationToken);
        var plainBytes = memory.ToArray();
        var integrityHash = hashService.ComputeSha256(plainBytes);
        var encryptedBytes = encryption.EncryptBytes(plainBytes);
        var saved = await storage.SaveAsync(userId, encryptedBytes, cancellationToken);

        try
        {
            var document = new StoredDocument
            {
                OriginalFileName = originalName,
                StoredFileName = saved.StoredFileName,
                RelativePath = saved.RelativePath,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                FileSizeBytes = file.Length,
                IntegrityHashSha256 = integrityHash,
                UserId = userId,
                FolderId = request.FolderId
            };
            db.Documents.Add(document);
            db.UploadLogs.Add(new UploadLog { UserId = userId, DocumentId = document.Id });
            await db.SaveChangesAsync(cancellationToken);

            var folderName = request.FolderId.HasValue
                ? await db.Folders.Where(x => x.Id == request.FolderId).Select(x => x.Name).SingleAsync(cancellationToken)
                : null;

            var response = new DocumentResponse(document.Id, document.OriginalFileName, document.ContentType,
                document.FileSizeBytes, document.IntegrityHashSha256, document.FolderId, folderName,
                document.CreatedAtUtc, document.UpdatedAtUtc);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, response);
        }
        catch
        {
            await storage.DeleteAsync(saved.RelativePath, cancellationToken);
            throw;
        }
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var document = await db.Documents.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (document is null) return NotFound();

        var encryptedBytes = await storage.ReadAsync(document.RelativePath, cancellationToken);
        var plainBytes = encryption.DecryptBytes(encryptedBytes);
        var currentHash = hashService.ComputeSha256(plainBytes);
        if (!string.Equals(currentHash, document.IntegrityHashSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("File integrity verification failed. The stored file may have been changed or corrupted.");

        return File(plainBytes, document.ContentType, document.OriginalFileName);
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var document = await db.Documents.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (document is null) return NotFound();

        var previewContentType = GetPreviewContentType(document.OriginalFileName);
        if (previewContentType is null)
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new
            {
                message = "Inline preview is available for PDF, TXT, JPG, JPEG, and PNG files. Download this file to view it in its native application."
            });

        var encryptedBytes = await storage.ReadAsync(document.RelativePath, cancellationToken);
        var plainBytes = encryption.DecryptBytes(encryptedBytes);
        var currentHash = hashService.ComputeSha256(plainBytes);
        if (!string.Equals(currentHash, document.IntegrityHashSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("File integrity verification failed. The stored file may have been changed or corrupted.");

        Response.Headers["Cache-Control"] = "no-store, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(plainBytes, previewContentType);
    }

    [HttpGet("{id:guid}/integrity")]
    public async Task<ActionResult<IntegrityResponse>> VerifyIntegrity(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var document = await db.Documents.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (document is null) return NotFound();

        var encryptedBytes = await storage.ReadAsync(document.RelativePath, cancellationToken);
        var plainBytes = encryption.DecryptBytes(encryptedBytes);
        var currentHash = hashService.ComputeSha256(plainBytes);
        var isValid = string.Equals(currentHash, document.IntegrityHashSha256, StringComparison.OrdinalIgnoreCase);
        return Ok(new IntegrityResponse(document.Id, isValid, document.IntegrityHashSha256, currentHash));
    }

    [HttpPut("{id:guid}/rename")]
    public async Task<ActionResult<DocumentResponse>> Rename(Guid id, RenameDocumentRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var document = await db.Documents.Include(x => x.Folder)
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (document is null) return NotFound();

        var newBaseName = Path.GetFileNameWithoutExtension(Path.GetFileName(request.NewFileName.Trim()));
        if (string.IsNullOrWhiteSpace(newBaseName)) return BadRequest(new { message = "A valid file name is required." });
        document.OriginalFileName = newBaseName + Path.GetExtension(document.OriginalFileName);
        document.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new DocumentResponse(document.Id, document.OriginalFileName, document.ContentType,
            document.FileSizeBytes, document.IntegrityHashSha256, document.FolderId, document.Folder?.Name,
            document.CreatedAtUtc, document.UpdatedAtUtc));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var document = await db.Documents.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (document is null) return NotFound();

        await storage.DeleteAsync(document.RelativePath, cancellationToken);
        db.Documents.Remove(document);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
    private static string? GetPreviewContentType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".txt" => "text/plain; charset=utf-8",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => null
    };

}
