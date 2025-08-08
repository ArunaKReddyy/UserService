namespace UserService.Application.DTOs.Response;

public class ForgotPasswordResponseDTO
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
}
