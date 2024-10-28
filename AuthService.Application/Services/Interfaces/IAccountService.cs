using AuthService.Application.Data.Dtos;
using AuthService.Core.Models;

namespace AuthService.Application.Services.Interfaces;

public interface IAccountService
{
    Task<User> GetSelf(string token);
    Task ResetPassword(Guid userId, ResetPasswordDto request);
}
