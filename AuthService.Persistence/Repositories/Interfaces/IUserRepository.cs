using AuthService.Core.Models;
using System.Linq.Expressions;

namespace AuthService.Persistence.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> CreateAsync(User user);
        public Task<IEnumerable<User>> Get(
            Expression<Func<User, bool>>? filter = null,
            Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null,
            string includeProperties = "");
    }
}
