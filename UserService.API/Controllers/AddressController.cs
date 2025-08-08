using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserService.API.DTO;
using UserService.Application.DTOs.Request;
using UserService.Application.Services;

namespace UserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("addresses")]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddOrUpdateAddress([FromBody] AddressDTO dto)
        {
            var addressId = await _userService.AddOrUpdateAddressAsync(dto);
            if (addressId == Guid.Empty)
                return BadRequest(ApiResponse<string>.FailResponse("Failed to add or update address."));

            return Ok(ApiResponse<Guid>.SuccessResponse(addressId, "Address saved successfully."));
        }

        [HttpGet("{userId}/addresses")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressDTO>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAddresses(Guid userId)
        {
            try
            {
                var addresses = await _userService.GetAddressesAsync(userId);
                return Ok(ApiResponse<IEnumerable<AddressDTO>>.SuccessResponse(addresses));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error fetching addresses.", new List<string> { ex.Message }));
            }
        }

        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.NotFound)]
        [HttpPost("delete-address")]
        public async Task<IActionResult> DeleteAddress([FromBody] DeleteAddressDTO dto)
        {
            try
            {
                var deleted = await _userService.DeleteAddressAsync(dto.UserId, dto.AddressId);
                if (!deleted)
                    return BadRequest(ApiResponse<string>.FailResponse("Address not found or deletion failed."));

                return Ok(ApiResponse<string>.SuccessResponse("Address deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error deleting address.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{userId}/address/{addressId}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserAddress(Guid userId, Guid addressId)
        {
            try
            {
                var address = await _userService.GetAddressByUserIdAndAddressIdAsync(userId, addressId);
                if (address != null)
                    return Ok(ApiResponse<AddressDTO>.SuccessResponse(address));

                return NotFound(ApiResponse<string>.FailResponse("Address not found."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.FailResponse("Error fetching addresses.", new List<string> { ex.Message }));
            }
        }

    }
}
