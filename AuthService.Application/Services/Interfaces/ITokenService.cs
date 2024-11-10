using System.Security.Claims;
using AuthService.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> Generate(User user);
        string GenerateEmailToken(User user);
        string GenerateRefreshToken();
        Task<TokenValidationResult> Validate(string token, bool lifetime = false);
        Task<ClaimsIdentity> GetClaimsIdentity(string token);
    }
}
