using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services.Interfaces;
using AuthService.Core.Exceptions;
using AuthService.Core.Models;

namespace AuthService.Application.Services;

public class AccountService(ITokenService tokenService, UserServiceClient userServiceClient)
    : IAccountService
{
    private readonly ITokenService _tokenService = tokenService;
    private readonly UserServiceClient _userServiceClient = userServiceClient;

    public async Task<User> GetSelf(string token)
    {
        var claimsIdentity = await _tokenService.GetClaims(token);
        var userId = new Guid(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "userId").Value);
        var user = await _userServiceClient.GetUser(userId);

        return user;
    }

    public async Task ResetPassword(Guid userId, ResetPasswordDto request)
    {
        var user = await _userServiceClient.GetUser(userId);

        if (!BCrypt.Net.BCrypt.EnhancedVerify(request.CurrentPassword, user.PasswordHash))
        {
            throw new AuthServiceExceptions(
                "Invalid password",
                AuthServiceExceptionTypes.INVALID_PASSWORD
            );
        }

        user.PasswordHash = request.NewPassword;

        await _userServiceClient.UpdateUser(user);
    }
}
