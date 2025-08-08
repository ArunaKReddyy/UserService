using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Services;

public interface IUserService
{
    Task<Guid> AddOrUpdateAddressAsync(AddressDTO dto);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> DeleteAddressAsync(Guid userId, Guid addressId);
    Task<ForgotPasswordResponseDTO?> ForgotPasswordAsync(string email);
    Task<AddressDTO> GetAddressByUserIdAndAddressIdAsync(Guid userId, Guid addressId);
    Task<IEnumerable<AddressDTO>> GetAddressesAsync(Guid userId);
    Task<ProfileDTO?> GetProfileAsync(Guid userId);
    Task<bool> IsUserExistsAsync(Guid userId);
    Task<LoginResponseDTO?> LoginAsync(LoginDTO dto, string iPAddress, string userAgent);
    Task<RefreshTokenResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO dto, string iPAddress, string userAgent);
    Task<bool> RegisterUser(RegisterUserDTo registerUserDTo);
    Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string v);
    Task<EmailConfirmationTokenResponseDTO?> SendConfirmationEmailAsync(string email);
    Task<bool> UpdateProfileAsync(UpdateProfileDTO dto);
    Task<bool> VerifyConfirmationEmailAsync(ConfirmEmailDTO dto);
}
