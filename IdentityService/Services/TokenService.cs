using IdentityService.Data;
using Microsoft.IdentityModel.Tokens;
using ProviGo.Common.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services;
public class TokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) { _config = config; }

    public string Create(User user)
    {
        var claims = new List<Claim>
    {
        new Claim("tenantId", user.TenantId.ToString()),
        new Claim("userId", user.UserId.ToString()),
        new Claim(ClaimTypes.Role, user.UserRole.RoleName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}
