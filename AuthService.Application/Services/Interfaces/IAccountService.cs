using AuthService.Core.Models;

namespace AuthService.Application.Services.Interfaces;

public interface IAccountService
{
    Task<User> GetSelf(string token);
}
