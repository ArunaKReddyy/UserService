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
    Task<string?> GeneratePasswordResetTokenAsync(User user);
    Task<bool> ResetPasswordAsync(User user, string token, string newPassword);
    Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<Guid> AddOrUpdateAddressAsync(Address address);
    Task<List<Address>> GetAddressesByUserIdAsync(Guid userId);
    Task<bool> DeleteAddressAsync(Guid userId, Guid addressId);
    Task<Address?> GetAddressByUserIdAndAddressIdAsync(Guid userId, Guid addressId);
}
