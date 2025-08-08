using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByUserNameAsync(string userName);
    Task<User?> FindByIdAsync(Guid id); 
    Task<bool> CreateUserAsync(User user, string password);
    Task<bool> AssignRoleAsync(User userId, string roleName);
    Task<string?> GenerateEmailConfirmationTokenAsync(User user);
    Task<bool> VerifyConfirmaionEmailAsync(User user, string token);
    Task<bool> UpdateUserAsync(User user);
}
