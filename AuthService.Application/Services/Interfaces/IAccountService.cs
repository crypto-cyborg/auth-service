using AuthService.Application.Data.Dtos;
using AuthService.Core.Models;

namespace AuthService.Application.Services.Interfaces;

public interface IAccountService
{
    Task<User> GetSelf(Guid id);
    Task ConfirmEmail(string token);
    Task ResetPassword(string token, ResetPasswordDto data);
}
