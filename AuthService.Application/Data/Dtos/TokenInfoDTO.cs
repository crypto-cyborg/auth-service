namespace AuthService.Application.Data.Dtos
{
    public record TokenInfoDTO(
        string AccessToken, 
        string RefreshToken 
    );
}
