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

    #region Address
    public async Task<Guid> AddOrUpdateAddressAsync(AddressDTO dto)
    {
        var address = new Address
        {
            Id = dto.Id ?? Guid.NewGuid(),
            UserId = dto.userId,
            AddresLine1 = dto.AddressLine1,
            AddresLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            IsDefaultBilling = dto.IsDefaultBilling,
            IsDefaultShipping = dto.IsDefaultShipping
        };

        var addressId = await _userRepository.AddOrUpdateAddressAsync(address);
        return addressId;
    }
    public async Task<IEnumerable<AddressDTO>> GetAddressesAsync(Guid userId)
    {
        var addresses = await _userRepository.GetAddressesByUserIdAsync(userId);
        return addresses.Select(a => new AddressDTO
        {
            Id = a.Id,
            AddressLine1 = a.AddresLine1,
            AddressLine2 = a.AddresLine2,
            City = a.City,
            State = a.State,
            PostalCode = a.PostalCode,
            Country = a.Country,
            IsDefaultBilling = a.IsDefaultBilling,
            IsDefaultShipping = a.IsDefaultShipping
        });
    }
    public async Task<bool> DeleteAddressAsync(Guid userId, Guid addressId)
    {
        return await _userRepository.DeleteAddressAsync(userId, addressId);
    }
    public async Task<AddressDTO?> GetAddressByUserIdAndAddressIdAsync(Guid userId, Guid addressId)
    {
        var address = await _userRepository.GetAddressByUserIdAndAddressIdAsync(userId, addressId);
        if (address != null)
        {
            return new AddressDTO
            {
                Id = address.Id,
                AddressLine1 = address.AddresLine1,
                AddressLine2 = address.AddresLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                IsDefaultBilling = address.IsDefaultBilling,
                IsDefaultShipping = address.IsDefaultShipping
            };
        }

        return null;
    }

    #endregion
}
