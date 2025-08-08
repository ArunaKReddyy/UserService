using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UAParser;
using UserService.API.DTO;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var UserAgent = GetNormalizedUserAgent();
            var loginResponse = await _userService.LoginAsync(dto, IPAddress, UserAgent);

            // Always return LoginResponseDTO wrapped in ApiResponse
            if (!string.IsNullOrEmpty(loginResponse.ErrorMessage))
            {
                // Failure case - Success = false, return DTO with error message
                loginResponse.Succeeded = false; // Add this property if missing
                return Unauthorized(ApiResponse<LoginResponseDTO>.FailResponse(loginResponse.ErrorMessage, errors: null, data: loginResponse));
            }

            // Success or requires 2FA
            loginResponse.Succeeded = true; // Make sure this is set on success path as well
            return Ok(ApiResponse<LoginResponseDTO>.SuccessResponse(loginResponse,
                loginResponse.RequiresTwoFactor ? "Two-factor authentication required." : "Login successful."));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO dto)
        {
            var IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var UserAgent = GetNormalizedUserAgent();

            var refreshTokenResponse = await _userService.RefreshTokenAsync(dto, IPAddress, UserAgent);

            if (!string.IsNullOrEmpty(refreshTokenResponse.ErrorMessage))
                return Unauthorized(ApiResponse<string>.FailResponse(refreshTokenResponse.ErrorMessage));

            return Ok(ApiResponse<RefreshTokenResponseDTO>.SuccessResponse(refreshTokenResponse, "Token refreshed successfully."));
        }

        [HttpPost("revoke-token")]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDTO dto)
        {
            try
            {
                var success = await _userService.RevokeRefreshTokenAsync(dto.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");
                if (!success)
                    return BadRequest(ApiResponse<string>.FailResponse("Invalid token or token already revoked."));

                return Ok(ApiResponse<string>.SuccessResponse("Token revoked successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error revoking token.", new List<string> { ex.Message }));
            }
        }
        [HttpGet("{userId}/exists")]
        public async Task<IActionResult> UserExists(Guid userId)
        {
            bool exists = await _userService.IsUserExistsAsync(userId);

            var response = new ApiResponse<bool>
            {
                Success = true,
                Data = exists,
                Message = exists ? "User exists." : "User does not exist."
            };

            return Ok(response);
        }
        private string GetNormalizedUserAgent()
        {
            var userAgentRaw = HttpContext.Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrWhiteSpace(userAgentRaw))
                return "Unknown";

            try
            {
                var uaParser = Parser.GetDefault();
                ClientInfo clientInfo = uaParser.Parse(userAgentRaw);

                var browser = clientInfo.UA.Family ?? "UnknownBrowser";
                var browserVersion = clientInfo.UA.Major ?? "0";
                var os = clientInfo.OS.Family ?? "UnknownOS";

                return $"{browser}-{browserVersion}_{os}";
            }
            catch
            {
                // In case parsing fails, fallback to raw user agent or unknown
                return "Unknown";
            }
        }
    }
}
