using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserService.API.DTO;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Register(RegisterUserDTo registerUserDTo)
        {
            try
            {
                var result = await _userService.RegisterUser(registerUserDTo);
                if (!result)
                    return BadRequest(ApiResponse<string>.FailResponse("Registration failed. Email or username might already exist."));

                return Ok(ApiResponse<string>.SuccessResponse("User registered successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error during registration.", new List<string> { ex.Message }));
            }
        }
        [HttpPost("send-confirmation-email")]
        [ProducesResponseType(typeof(ApiResponse<EmailConfirmationTokenResponseDTO>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendConfirmationEmail([FromBody] EmailDTO dto)
        {
            try
            {
                var emailTokenResponse = await _userService.SendConfirmationEmailAsync(dto.Email);
                if (emailTokenResponse == null)
                    return NotFound(ApiResponse<string>.FailResponse("User with this email not found"));

                return Ok(ApiResponse<EmailConfirmationTokenResponseDTO>.SuccessResponse(emailTokenResponse, "Email confirmation token generated successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error generating confirmation token.", new List<string> { ex.Message }));
            }
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> VerifyConfirmationEmailAsync([FromBody] ConfirmEmailDTO dto)
        {
            try
            {
                var success = await _userService.VerifyConfirmationEmailAsync(dto);
                if (!success)
                    return BadRequest(ApiResponse<string>.FailResponse("Invalid confirmation token or user."));

                return Ok(ApiResponse<string>.SuccessResponse("Email confirmed successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error confirming email.", new List<string> { ex.Message }));
            }
        }
    }

}
