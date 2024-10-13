using AuthService.Application.Data;
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
}
