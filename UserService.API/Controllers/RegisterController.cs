using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserService.API.DTO;
using UserService.Application.DTOs;
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
    }
}
