using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Models;

namespace WebApplication3.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(UserClaimsDTO User)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                    new Claim("UserID", User.UserID.ToString()),
                    new Claim("RoleName", User.RoleName),
                    new Claim("UserName", User.UserName),
                    new Claim("FullName", User.FullName),
                    new Claim("RoleID", User.RoleID.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public object DecodedToken(string token)
        {
            string secret = _config["Jwt:Key"] ;
            var key = Encoding.UTF8.GetBytes(secret);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token); // Chỉ đọc, không validate

            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var claimsPrincipal = handler.ValidateToken(token, validations, out var tokenSecure);

            var claimsDictionary = claimsPrincipal.Claims
            .ToDictionary(c => c.Type, c => c.Value);

            return claimsDictionary;

        }

    }
}
