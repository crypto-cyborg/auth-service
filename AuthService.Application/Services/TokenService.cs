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
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;

        public TokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public async Task<string> Generate(User user)
        {
            Claim[] claims =
            [
                new("userId", user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Username),
                new(ClaimTypes.Role, string.Join(",", user.UserRoles)),
            ];

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddHours(_options.ExpiresHours)
            );

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }

        public string GenerateRandomToken()
        {
            var randomNumber = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        public async Task<ClaimsIdentity> GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = GetValidationParameters();

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenValidationResult = await tokenHandler.ValidateTokenAsync(
                token,
                tokenValidationParameters
            );

            if (!tokenValidationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid token");
            }

            return tokenValidationResult.ClaimsIdentity;
        }

        public async Task<string> GetUsername(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadJwtToken(token);
            var validationParams = GetValidationParameters();

            var tokenValidationResult = await handler.ValidateTokenAsync(token, validationParams);

            if (!tokenValidationResult.IsValid)
            {
                throw new SecurityTokenException("Invalid token");
            }

            return securityToken
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                .Value;
        }

        private TokenValidationParameters GetValidationParameters()
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
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
