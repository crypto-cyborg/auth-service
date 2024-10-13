using AuthService.Application.Data;
using Microsoft.AspNetCore.Http;

namespace AuthService.Application;

public interface ICookiesService
{
    void WriteToken(TokenData data, HttpContext context);
}
