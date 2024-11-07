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
    public class TokenService(IOptions<JwtOptions> options, IConfiguration configuration)
        : ITokenService
    {
        private readonly JwtOptions _options = options.Value;

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
                expires: DateTime.UtcNow.AddSeconds(_options.ExpiresMinutes)
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

        public async Task<TokenValidationResult> Validate(string token)
        {
            var validationParams = GetValidationParameters();
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

        public TokenInfoDTO? ReadToken(HttpContext context)
        {
            var name =
                configuration.GetSection("cookie-name").Value
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
