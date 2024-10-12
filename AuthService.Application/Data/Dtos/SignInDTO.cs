using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Data.Dtos
{
    public record SignInDTO([Required] string Username, [Required] string Password);
}
