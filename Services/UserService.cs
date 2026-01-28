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
            PasswordHash = Extensions.AuthenticationExtension.HashPassword(userDto.Password),
            IsAdmin = userDto.IsAdmin,
            UserId = Guid.NewGuid()
        };
        await _userRepository.AddUserAsync(user);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }

    public async Task<User?> GetUserByUserIdAsync(Guid userId)
    {
        return await _userRepository.GetUserByUserIdAsync(userId);
    }
}
