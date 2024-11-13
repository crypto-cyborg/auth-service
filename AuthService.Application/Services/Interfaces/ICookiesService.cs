using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using Microsoft.AspNetCore.Http;

namespace AuthService.Application.Services.Interfaces;

public interface ICookiesService
{
    void WriteToken(TokenData data, HttpContext context);
    TokenInfoDTO? ReadToken(HttpContext context);
    void DeleteToken(HttpContext context);
}
