using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using UserService.API.DTO;
using UserService.Application.DTOs;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailDTO dto)
        {
            try
            {
                var forgotPassword = await _userService.ForgotPasswordAsync(dto.Email);
                if (forgotPassword == null)
                    return NotFound(ApiResponse<string>.FailResponse("Email not found."));

                return Ok(ApiResponse<ForgotPasswordResponseDTO>.SuccessResponse(forgotPassword, "Password reset token sent to email."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error in forgot password process.", new List<string> { ex.Message }));
            }
        }
        
        // Reset Password (Forgot Password Flow)qqq
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            try
            {
                var success = await _userService.ResetPasswordAsync(dto.UserId, dto.Token, dto.NewPassword);
                if (!success)
                    return BadRequest(ApiResponse<string>.FailResponse("Invalid token or user."));

                return Ok(ApiResponse<string>.SuccessResponse("Password reset successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error resetting password.", new List<string> { ex.Message }));
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized(ApiResponse<string>.FailResponse("Invalid user token."));

                var success = await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
                if (!success)
                    return BadRequest(ApiResponse<string>.FailResponse("Password change failed."));

                return Ok(ApiResponse<string>.SuccessResponse("Password changed successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error changing password.", new List<string> { ex.Message }));
            }
        }
    }
}
