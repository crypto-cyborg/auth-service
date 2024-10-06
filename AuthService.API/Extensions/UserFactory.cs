
using AuthService.Data.Models;

namespace AuthService.API.Extensions
{
    public static class UserFactory
    {
        public static User Create(Guid userId,
            string username, string passwordHash,
            string email, string name, string surname)
        {
            return new User
            {
                Id = userId,
                Username = username,
                PasswordHash = passwordHash,
                Email = email,
                IsEmailConfirmed = false,
                FirstName = name,
                LastName = surname,
            };
        }
    }

}
