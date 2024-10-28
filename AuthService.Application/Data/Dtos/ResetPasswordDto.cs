namespace AuthService.Application.Data.Dtos;

public record class ResetPasswordDto
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string CofirmNewPassword { get; set; }
}
