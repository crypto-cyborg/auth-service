using AuthService.Core.Models;
using AuthService.Persistence.Repositories.Interfaces;
using System.Linq.Expressions;

namespace AuthService.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<bool> CreateAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> Get(Expression<Func<User, bool>>? filter = null, Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null, string includeProperties = "")
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            throw new NotImplementedException();
        }
    }
}
