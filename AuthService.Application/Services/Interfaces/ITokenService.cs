using System.Security.Claims;
using AuthService.Application.Data.Dtos;
using AuthService.Core.Models;
using Microsoft.AspNetCore.Http;

namespace AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> Generate(User user);
        string GenerateRandomToken();
        Task<ClaimsIdentity> GetClaimsIdentity(string token);
        TokenInfoDTO? ReadToken(HttpContext context);
    }
}
