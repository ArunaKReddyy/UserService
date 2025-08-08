using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ProfileDTO?> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null) return null;
        return new ProfileDTO()
        {
            UserId = user.Id,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Email = user.Email,
            LastLoginAt = user.LastLoginAt,
            UserName = user.UserName
        };

    }

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
        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null) return null;

        var result = await _userRepository.GenerateEmailConfirmationTokenAsync(user);
        if (result == null) return null;
        return new EmailConfirmationTokenResponseDTO
        {
            UserId = user.Id,
            Token = result,
        };
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileDTO dto)
    {
        var user = await _userRepository.FindByIdAsync(dto.UserId);
        if (user == null) return false;

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.ProfilePhotoUrl = dto.ProfilePhotoUrl;

        return await _userRepository.UpdateUserAsync(user);
    }

    public async Task<bool> VerifyConfirmationEmailAsync(ConfirmEmailDTO dto)
    {
        var user = await _userRepository.FindByIdAsync(dto.UserId);
        if (user == null)
            return false;

        var result = await _userRepository.VerifyConfirmaionEmailAsync(user, dto.Token);

        return result;
    }


    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
            return false;

        return await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<ForgotPasswordResponseDTO?> ForgotPasswordAsync(string email)
    {
        ForgotPasswordResponseDTO? forgotPasswordResponseDTO = null;

        var user = await _userRepository.FindByEmailAsync(email);
        if (user == null)
            return null;

        var token = await _userRepository.GeneratePasswordResetTokenAsync(user);

        if (token != null)
        {
            forgotPasswordResponseDTO = new ForgotPasswordResponseDTO()
            {
                UserId = user.Id,
                Token = token
            };
        }

        return forgotPasswordResponseDTO;
    }
    public async Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null) return false;

        return await _userRepository.ResetPasswordAsync(user, token, newPassword);
    }

}
