using System.Security.Claims;
using AIResumeAnalyzer.MVC.Helpers;
using AIResumeAnalyzer.MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class ApiUserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public ApiUserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Returns the currently logged-in user's data from MongoDB.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<object>.Error("User not authenticated."));

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.Error("User not found."));

            return Ok(ApiResponse<object>.Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.Phone,
                user.Location,
                user.CreatedAt
            }, "User retrieved successfully."));
        }

        /// <summary>
        /// Updates the currently logged-in user's profile (name, phone, location).
        /// </summary>
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<object>.Error("User not authenticated."));

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.Error("User not found."));

            // Update only the allowed fields
            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;

            user.Phone = request.Phone;
            user.Location = request.Location;

            await _userRepository.UpdateUserAsync(userId, user);

            return Ok(ApiResponse<object>.Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Phone,
                user.Location
            }, "Profile updated successfully."));
        }

        /// <summary>
        /// Updates the currently logged-in user's application and notification settings.
        /// </summary>
        [HttpPut("update-settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] AIResumeAnalyzer.MVC.Models.UserSettings request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<object>.Error("User not authenticated."));

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.Error("User not found."));

            user.Settings = request;

            await _userRepository.UpdateUserAsync(userId, user);

            return Ok(ApiResponse<object>.Ok(user.Settings, "Settings updated successfully."));
        }
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
    }
}
