using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Core.Models;

namespace AuthService.Application.Services.Interfaces;

public interface IIdentityService
{
    Task<User> SignUp(SignUpDTO request);
    Task<(TokenData? tokenData, Status status)> SignIn(string username, string password);
    Task<(TokenData? tokenData, Status status)> RefreshTokenAsync(TokenInfoDTO data);
    Task SignOut(TokenInfoDTO tokens);
}
