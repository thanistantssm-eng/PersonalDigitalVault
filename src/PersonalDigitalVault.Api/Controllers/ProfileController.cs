using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalDigitalVault.Api.Data;
using PersonalDigitalVault.Api.DTOs;
using PersonalDigitalVault.Api.Extensions;

namespace PersonalDigitalVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProfileController(VaultDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var id = User.GetUserId();
        var user = await db.Users.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);
        return Ok(new ProfileResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.Role, user.CreatedAtUtc));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> Update(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var id = User.GetUserId();
        var user = await db.Users.SingleAsync(x => x.Id == id, cancellationToken);
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new ProfileResponse(user.Id, user.FullName, user.Email, user.PhoneNumber, user.Role, user.CreatedAtUtc));
    }
}
