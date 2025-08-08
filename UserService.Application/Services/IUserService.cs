using UserService.Application.DTOs;

namespace UserService.Application.Services;

public interface IUserService
{
    Task<bool> RegisterUser(RegisterUserDTo registerUserDTo);
}
