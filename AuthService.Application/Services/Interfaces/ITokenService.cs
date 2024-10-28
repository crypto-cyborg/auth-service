using System.Security.Claims;
using AuthService.Core.Models;
using Microsoft.AspNetCore.Http;

namespace AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> Generate(User user);
        string GenerateRandomToken();
        Task<ClaimsIdentity> GetClaims(string token);
        string? ReadToken(HttpContext context);
    }
}
