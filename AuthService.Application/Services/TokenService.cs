using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Core.Exceptions;
using AuthService.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly IConfiguration _configuration;

        public TokenService(IOptions<JwtOptions> options, IConfiguration configuration)
        {
            _options = options.Value;
            _configuration = configuration;
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

        public async Task<ClaimsIdentity> GetClaims(string token)
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

        public TokenInfoDTO? ReadToken(HttpContext context)
        {
            var name =
                _configuration.GetSection("cookie-name").Value
                ?? throw new AuthServiceExceptions(
                    "Cookies configuration not found",
                    AuthServiceExceptionTypes.IVALID_COOKIE_CONFIGURATION
                );

            var data = new TokenInfoDTO
            {
                AccessToken =
                    context.Request.Cookies[name] ?? throw new Exception("Cookie config not found"),
                RefreshToken =
                    context.Request.Cookies[$"refresh-{name}"]
                    ?? throw new Exception("Cookie config not found"),
            };

            return data;
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
