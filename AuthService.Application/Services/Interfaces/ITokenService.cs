using AuthService.Core.Models;
using System.Security.Claims;

namespace AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> Generate(User user);
        string GenerateRandomToken();
        Task<ClaimsIdentity> GetPrincipalFromExpiredToken(string token);
    }
}
