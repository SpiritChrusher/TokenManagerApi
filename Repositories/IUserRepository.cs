using TokenManagerApi.Models;

namespace TokenManagerApi.Repositories;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByUserIdAsync(Guid userId);
}
