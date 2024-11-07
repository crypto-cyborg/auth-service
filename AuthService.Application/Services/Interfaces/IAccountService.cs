using AuthService.Application.Data.Dtos;
using AuthService.Core.Models;

namespace AuthService.Application.Services.Interfaces;

public interface IAccountService
{
    Task<User> GetSelf(string token);
    Task ConfirmEmail(string token);
}
