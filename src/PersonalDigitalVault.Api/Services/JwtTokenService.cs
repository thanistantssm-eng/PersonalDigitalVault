using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PersonalDigitalVault.Api.Models;

namespace PersonalDigitalVault.Api.Services;

public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(VaultUser user);
}

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public JwtTokenResult CreateToken(VaultUser user)
    {
        var secret = configuration["Jwt:Secret"]
                     ?? throw new InvalidOperationException("JWT secret is not configured.");
        if (secret.Length < 32)
            throw new InvalidOperationException("JWT secret must be at least 32 characters.");

        var issuer = configuration["Jwt:Issuer"] ?? "PersonalDigitalVault.Api";
        var audience = configuration["Jwt:Audience"] ?? "PersonalDigitalVault.Client";
        var expiryMinutes = configuration.GetValue<int?>("Jwt:ExpiryMinutes") ?? 60;
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
