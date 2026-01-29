using TokenManagerApi.Models;

namespace TokenManagerApi.Services;

public interface IUserService
{
    Task RegisterUserAsync(Dtos.UserDto userDto);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByUserIdAsync(string userId);
    /// <summary>
    /// Authenticates a user by username and password. Returns the user if valid, otherwise null.
    /// </summary>
    Task<User?> AuthenticateUserAsync(string username, string password);
}
