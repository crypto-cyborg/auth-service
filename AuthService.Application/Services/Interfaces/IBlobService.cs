using Microsoft.AspNetCore.Http;

namespace AuthService.Application.Services.Interfaces;

public interface IBlobService
{
    Task<string> UploadImage(IFormFile image);
}