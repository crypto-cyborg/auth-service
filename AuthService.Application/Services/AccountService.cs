using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services.Interfaces;
using AuthService.Core.Models;

namespace AuthService.Application.Services;

public class AccountService(ITokenService tokenService, UserServiceClient userServiceClient)
    : IAccountService
{
    private readonly ITokenService _tokenService = tokenService;
    private readonly UserServiceClient _userServiceClient = userServiceClient;

    public async Task<User> GetSelf(string token)
    {
        var username = await _tokenService.GetUsername(token);
        var user = await _userServiceClient.GetUser(username);

        return user;
    }
}
