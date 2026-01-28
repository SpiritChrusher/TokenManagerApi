using Microsoft.EntityFrameworkCore;
using TokenManagerApi.Models;

namespace TokenManagerApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TokenManagerDbContext _db;
    public UserRepository(TokenManagerDbContext db)
    {
        _db = db;
    }

    public async Task AddUserAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _db.Users.SingleOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByUserIdAsync(Guid userId)
    {
        return await _db.Users.SingleOrDefaultAsync(u => u.UserId == userId);
    }
}
