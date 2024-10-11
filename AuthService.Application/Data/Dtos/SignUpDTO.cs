using System.ComponentModel.DataAnnotations;

namespace AuthService.Persistence.Data.Dtos
{
    public record SignUpDTO
    (
        [Required] string Username,
        [Required] string Password,
        [Required] string Email,
        [Required] string Name,
        [Required] string Surname
    );
}
