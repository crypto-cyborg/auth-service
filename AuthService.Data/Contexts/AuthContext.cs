using AuthService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data.Contexts
{
    public class AuthContext(DbContextOptions<AuthContext> opts) : DbContext(opts)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
    }
}
