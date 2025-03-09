using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Services.Interfaces;
using AuthService.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Services;

public class CookiesService : ICookiesService
{
    private readonly CookiesName _cookiesName;

    public CookiesService(IOptions<CookiesName> cookiesOptions)
    {
        _cookiesName = cookiesOptions.Value;
    }

    public void WriteToken(TokenData data, HttpContext context)
    {
        context.Response.Cookies.Append(_cookiesName.AccessToken, data.AccessToken);
        context.Response.Cookies.Append(_cookiesName.RefreshToken, data.RefreshToken);
    }

    public TokenInfoDTO? ReadToken(HttpContext context)
    {
        var accessToken = context.Request.Cookies[_cookiesName.AccessToken];
        var refreshToken = context.Request.Cookies[_cookiesName.RefreshToken];

        if (accessToken is null || refreshToken is null) return default;

        var data = new TokenInfoDTO(accessToken, refreshToken);

        return data;
    }

    public void DeleteToken(HttpContext context)
    {
        context.Response.Cookies.Delete(_cookiesName.AccessToken);
        context.Response.Cookies.Delete(_cookiesName.RefreshToken);
    }
}