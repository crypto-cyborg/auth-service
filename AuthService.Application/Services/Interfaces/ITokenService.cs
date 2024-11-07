using System.Security.Claims;
using AuthService.Application.Data.Dtos;
using AuthService.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> Generate(User user);
        string GenerateEmailToken(User user);
        string GenerateRefreshToken();
        Task<TokenValidationResult> Validate(string token);
        Task<ClaimsIdentity> GetClaimsIdentity(string token);
        TokenInfoDTO? ReadToken(HttpContext context);
    }
}
