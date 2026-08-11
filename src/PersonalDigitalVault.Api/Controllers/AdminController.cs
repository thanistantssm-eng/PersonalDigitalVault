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
[Authorize(Roles = UserRole.Administrator)]
public sealed class AdminController(VaultDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> Dashboard(CancellationToken cancellationToken)
    {
        var result = new DashboardResponse(
            await db.Users.CountAsync(cancellationToken),
            await db.UploadLogs.LongCountAsync(cancellationToken),
            await db.Documents.CountAsync(cancellationToken),
            await db.CredentialRecords.CountAsync(cancellationToken),
            DateTime.UtcNow);
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AdminUserResponse(x.Id, x.FullName, x.Email, x.PhoneNumber, x.Role, x.IsActive, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPatch("users/{id:guid}/status")]
    public async Task<ActionResult<AdminUserResponse>> UpdateStatus(
        Guid id,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (id == User.GetUserId() && !request.IsActive)
            return BadRequest(new { message = "An administrator cannot deactivate their own current account." });

        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null) return NotFound();
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new AdminUserResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.Role, user.IsActive, user.CreatedAtUtc));
    }

    [HttpGet("settings")]
    public async Task<ActionResult<IReadOnlyList<ApplicationSetting>>> GetSettings(CancellationToken cancellationToken) =>
        Ok(await db.ApplicationSettings.AsNoTracking().OrderBy(x => x.Key).ToListAsync(cancellationToken));

    [HttpPut("settings/{key}")]
    public async Task<ActionResult<ApplicationSetting>> UpsertSetting(
        string key,
        UpdateSettingRequest request,
        CancellationToken cancellationToken)
    {
        key = key.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100)
            return BadRequest(new { message = "Setting key must contain 1 to 100 characters." });

        var setting = await db.ApplicationSettings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is null)
        {
            setting = new ApplicationSetting { Key = key, Value = request.Value };
            db.ApplicationSettings.Add(setting);
        }
        else
        {
            setting.Value = request.Value;
            setting.UpdatedAtUtc = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(cancellationToken);
        return Ok(setting);
    }
}
