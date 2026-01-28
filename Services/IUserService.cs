using TokenManagerApi.Models;

namespace TokenManagerApi.Services;

public interface IUserService
{
    Task RegisterUserAsync(TokenManagerApi.Dtos.UserDto userDto);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByUserIdAsync(Guid userId);
}
