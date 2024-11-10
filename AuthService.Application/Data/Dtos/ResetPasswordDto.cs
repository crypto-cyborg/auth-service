using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Data.Dtos;

public record ResetPasswordDto(
    [Required] string CurrentPassword, 
    [Required] string NewPassword,
    [Required] string CofirmNewPassword 
);
