using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
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

    public async Task<EmailConfirmationTokenResponseDTO?> SendConfirmationEmailAsync(string email)
    {
       var user= await _userRepository.FindByEmailAsync(email);
        if (user == null) return null;

        var result = await _userRepository.GenerateEmailConfirmationTokenAsync(user);
        if (result == null) return null;
        return new EmailConfirmationTokenResponseDTO
        {
            UserId = user.Id,
            Token = result,
        };
    }

    public async Task<bool> VerifyConfirmationEmailAsync(ConfirmEmailDTO dto)
    {
        var user = await _userRepository.FindByIdAsync(dto.UserId);
        if (user == null)
            return false;

        var result = await _userRepository.VerifyConfirmaionEmailAsync(user, dto.Token);
        //if (result)
        //{
        //    user.IsActive = true;
        //    await _userRepository.UpdateUserAsync(user);
        //}
        return result;
    }
}
