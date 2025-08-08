using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<bool> RegisterUser(RegisterUserDTo registerUserDTo)
    {
        if (await _userRepository.FindByEmailAsync(registerUserDTo.Email) != null) return false;
        if (await _userRepository.FindByUserNameAsync(registerUserDTo.UserName) != null) return false;

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = registerUserDTo.UserName,
            Email = registerUserDTo.Email,
            PhoneNumber = registerUserDTo.PhoneNumber,
            FullName = registerUserDTo.FullName,
            IsEmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };
        var created = await _userRepository.CreateUserAsync(user, registerUserDTo.Password);

        if (!created) return false;

        await _userRepository.AssignRoleAsync(user, "Customer");
        return true;
    }
}
