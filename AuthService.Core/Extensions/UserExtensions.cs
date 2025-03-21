using AuthService.Core.Models;

namespace AuthService.Core.Extesions;

public static class UserExtensions
{
    public record UserReadDto(
            Guid Id,
            string Username,
            string? ImageUrl,
            string Email,
            bool IsEmailConfirmed,
            string ApiKey,
            string FirstName,
            string LastName,
            IEnumerable<RoleExtensions.RoleReadDto> Roles);

    public static UserReadDto MapToResponse(this User user)
        => new(user.Id,
                user.Username,
                user.ImageUrl,
                user.Email,
                user.IsEmailConfirmed,
                user.ApiKey,
                user.FirstName,
                user.LastName,
                user.Roles.MapToResponse());
}
