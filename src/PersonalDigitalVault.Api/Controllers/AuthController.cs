using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalDigitalVault.Api.Data;
using PersonalDigitalVault.Api.DTOs;
using PersonalDigitalVault.Api.Models;
using PersonalDigitalVault.Api.Services;

namespace PersonalDigitalVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    VaultDbContext db,
    IPasswordHasher<VaultUser> passwordHasher,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
            return Conflict(new { message = "An account with this email already exists." });

        var user = new VaultUser
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Role = UserRole.User,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var token = jwtTokenService.CreateToken(user);
        return Created(string.Empty, new AuthResponse(
            token.Token, token.ExpiresAtUtc, user.Id, user.FullName, user.Email, user.Role));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });
        if (!user.IsActive)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "This account is inactive." });

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var token = jwtTokenService.CreateToken(user);
        return Ok(new AuthResponse(
            token.Token, token.ExpiresAtUtc, user.Id, user.FullName, user.Email, user.Role));
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout() =>
        Ok(new { message = "Logout successful. Remove the JWT from the client application." });
}
