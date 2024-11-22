using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Data.Dtos
{
    public record SignUpDTO(
        [Required] string Username,
        [Required] string Password,
        [Required] string ConfirmPassword,
        [Required] string Email,
        [Required] string ApiKey,
        [Required] string SecretKey,
        [Required] string FirstName,
        [Required] string LastName
    );
}
