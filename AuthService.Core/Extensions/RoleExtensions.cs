using AuthService.Core.Models;

namespace AuthService.Core.Extesions;

public static class RoleExtensions
{
    public record RoleReadDto(int Id, string Name);

    public static RoleReadDto MapToResponse(this Role role) => new(role.Id, role.Name);

    public static IEnumerable<RoleReadDto> MapToResponse(this IEnumerable<Role> roles) =>
        roles.Select(r => r.MapToResponse());
}