using System.Security.Claims;
using AuthService.Core.Models;

namespace AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> Generate(User user);
        string GenerateRandomToken();
        Task<ClaimsIdentity> GetPrincipalFromExpiredToken(string token);
        string GetUsername(string token);
    }
}
