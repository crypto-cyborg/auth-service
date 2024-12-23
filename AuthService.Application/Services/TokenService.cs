using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Data;
using AuthService.Application.Interfaces;
using AuthService.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Services
{
    public class TokenService(IOptions<JwtOptions> options)
        : ITokenService
    {
        private readonly JwtOptions _options = options.Value;

        public async Task<string> Generate(User user)
        {
            List<Claim> claims =
            [
                new("userId", user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Username),
            ];
            
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddSeconds(15)
            );

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }

        public string GenerateEmailToken(User user)
        {
            Claim[] claims =
            [
                new("userId", user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
            ];
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddMinutes(5));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        public async Task<TokenValidationResult> Validate(string token, bool lifetime = false)
        {
            var validationParams = GetValidationParameters();
            validationParams.ValidateLifetime = lifetime;

            var handler = new JwtSecurityTokenHandler();

            return await handler.ValidateTokenAsync(token, validationParams);
        }

        public async Task<ClaimsIdentity> GetClaimsIdentity(string token)
        {
            var validationResult = await Validate(token);

            if (!validationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid token");
            }

            return validationResult.ClaimsIdentity;
        }

        private TokenValidationParameters GetValidationParameters()
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_options.SecretKey)
                ),
                RoleClaimType = ClaimTypes.Role,
            };

            return parameters;
        }
    }
}
