﻿using System.Security.Claims;
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
    public async Task<User> GetSelf(Guid id)
    {
        Console.WriteLine(id);
        var user = await userServiceClient.GetUser(id);

        return user;
    }

    public async Task ConfirmEmail(string token)
    {
        var claimsIdentity = await tokenService.GetClaimsIdentity(token);
        var userId = new Guid(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "userId")!.Value);
        var userEmail = claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;
        var user = await userServiceClient.GetUser(userId);

        if (user is null)
        {
            throw new AuthServiceExceptions(
                "Required user does not exist",
                AuthServiceExceptionTypes.USER_NOT_FOUND
            );
        }

        if (user.Email != userEmail)
        {
            // TODO    
        }
        user.IsEmailConfirmed = true;
        await userServiceClient.UpdateUser(user);
    }

    public async Task ResetPassword(string token, ResetPasswordDto data)
    {
        var claimsIdentity = await tokenService.GetClaimsIdentity(token);
        var userId = new Guid(claimsIdentity.Claims.FirstOrDefault(c => c.Type == "userId")!.Value);

        var user = await userServiceClient.GetUser(userId);

        if (user is null)
        {
            throw new AuthServiceExceptions(
                "Required user does not exist",
                AuthServiceExceptionTypes.USER_NOT_FOUND
            );
        }

        if (!BCrypt.Net.BCrypt.EnhancedVerify(data.CurrentPassword, user.PasswordHash))
        {
            throw new AuthServiceExceptions(
                "Invalid password",
                AuthServiceExceptionTypes.INVALID_PASSWORD
            );
        }

        user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(data.NewPassword);

        await userServiceClient.UpdateUser(user);
    }
}
