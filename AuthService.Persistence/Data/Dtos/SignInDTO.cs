using System.ComponentModel.DataAnnotations;

namespace AuthService.Persistence.Data.Dtos
{
    public record SignInDTO
    (
        [Required] string Username,
        [Required] string Password
    );
}
