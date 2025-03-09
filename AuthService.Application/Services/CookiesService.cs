using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Services.Interfaces;
using AuthService.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application;

public class CookiesService : ICookiesService
{
    private readonly IConfiguration _configuration;

    public CookiesService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void WriteToken(TokenData data, HttpContext context)
    {
        var cookieKey =
            _configuration.GetSection("cookie-name").Value
            ?? throw new Exception("Cannot find cookie key");

        context.Response.Cookies.Append(cookieKey, data.AccessToken);
        context.Response.Cookies.Append($"refresh-{cookieKey}", data.RefreshToken);
    }

    public TokenInfoDTO? ReadToken(HttpContext context)
    {
        var name =
            _configuration.GetSection("cookie-name").Value
            ?? throw new AuthServiceExceptions(
                "Cookies configuration not found",
                AuthServiceExceptionTypes.IVALID_COOKIE_CONFIGURATION
            );

        var data = new TokenInfoDTO(
            context.Request.Headers["accessToken"],
            context.Request.Cookies[$"refresh-token"]
            ?? throw new Exception("Cookie config not found"));

        return data;
    }
    public void DeleteToken(HttpContext context)
    {
        var name =
            _configuration.GetSection("cookie-name").Value
            ?? throw new AuthServiceExceptions(
                "Cookies configuration not found",
                AuthServiceExceptionTypes.IVALID_COOKIE_CONFIGURATION
            );

        context.Response.Cookies.Delete(name);
        context.Response.Cookies.Delete($"refresh-{name}");
    }
}
