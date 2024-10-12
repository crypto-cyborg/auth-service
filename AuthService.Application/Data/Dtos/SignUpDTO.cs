using System.ComponentModel.DataAnnotations;

namespace AuthService.Persistence.Data.Dtos
{
    public record SignUpDTO(
        [Required] string Username,
        [Required] string Password,
        [Required] string ConfirmPassword,
        [Required] string Email,
        [Required] string FirstName,
        [Required] string LastName
    );
}
