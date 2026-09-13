using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Scholarship.Api.Security;

public interface IJwtTokenService
{
    string GenerateAccessToken(ulong id, string username, string role, ulong? instituteId = null, string? studentCode = null);
    string GenerateRefreshToken();
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(ulong id, string username, string role, ulong? instituteId = null, string? studentCode = null)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? "ScholarshipSystemSecureJwtSuperSecretKey2026Min512BitsLongForProductionSecurityKey!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(studentCode))
        {
            claims.Add(new Claim("student_code", studentCode));
        }

        if (instituteId.HasValue)
        {
            claims.Add(new Claim("institute_id", instituteId.Value.ToString()));
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "Scholarship.Api";
        var audience = _configuration["Jwt:Audience"] ?? "Scholarship.Web";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int exp) ? exp : 60;

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
