using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Services;

public interface IUserService
{
    Task<bool> RegisterUser(RegisterUserDTo registerUserDTo);
    Task<EmailConfirmationTokenResponseDTO?> SendConfirmationEmailAsync(string email);
    Task<bool> VerifyConfirmationEmailAsync(ConfirmEmailDTO dto);
}
