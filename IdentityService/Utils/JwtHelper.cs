using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ProviGo.Common.Models;
using System.Text;

namespace IdentityService.Utils
{
    public static class JwtHelper
    {
        public static string GenerateToken(User user, IConfiguration config)
        {
            var claims = new[]
{
              new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
              new Claim(ClaimTypes.Name, user.Email),
              new Claim(ClaimTypes.Role, user.UserRole?.RoleName ?? "user"),  // optional: add role name
              new Claim("roleId", user.RoleId.ToString()) // ✅ custom claim for RoleId
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static TokenValidationParameters GetTokenValidationParams(IConfiguration config) => new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
        };
    }

}
