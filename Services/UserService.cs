using TokenManagerApi.Models;
using TokenManagerApi.Repositories;

namespace TokenManagerApi.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task RegisterUserAsync(TokenManagerApi.Dtos.UserDto userDto)
    {
        var user = new User {
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = HashPassword(userDto.Password),
            IsAdmin = userDto.IsAdmin,
            UserId = Guid.NewGuid().ToString()
        };
        await _userRepository.AddUserAsync(user);
    }
    public async Task<User?> AuthenticateUserAsync(string username, string password)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);
        if (user is null)
            return null;
        if (!VerifyPassword(password, user.PasswordHash))
            return null;
        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task<User?> GetUserByUserIdAsync(string userId)
    {
        return await _userRepository.GetUserByUserIdAsync(userId);
    }

        private static string HashPassword(string password)
    {
        // TODO: Use a secure hash function in production
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
