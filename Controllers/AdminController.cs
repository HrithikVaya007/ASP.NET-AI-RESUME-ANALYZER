using AIResumeAnalyzer.MVC.Helpers;
using AIResumeAnalyzer.MVC.Models;
using AIResumeAnalyzer.MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ResumeRepository _resumeRepository;
        private readonly InterviewSessionRepository _sessionRepository;

        public AdminController(UserRepository userRepository, ResumeRepository resumeRepository, InterviewSessionRepository sessionRepository)
        {
            _userRepository = userRepository;
            _resumeRepository = resumeRepository;
            _sessionRepository = sessionRepository;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            if (!IsAdmin()) return NotFound(); // Returns 404 instead of 403 to hide admin features from normal users

            var users = await _userRepository.GetAllUsersAsync();
            return Ok(ApiResponse<List<User>>.Ok(users, "Users fetched successfully."));
        }

        [HttpGet("resumes")]
        public async Task<IActionResult> GetAllResumes()
        {
            if (!IsAdmin()) return NotFound();

            var resumes = await _resumeRepository.GetAllResumesAsync();
            return Ok(ApiResponse<List<Resume>>.Ok(resumes, "Resumes fetched successfully."));
        }

        [HttpGet("interviews")]
        public async Task<IActionResult> GetAllInterviews()
        {
            if (!IsAdmin()) return NotFound();

            var sessions = await _sessionRepository.GetAllSessionsAsync();
            return Ok(ApiResponse<List<InterviewSession>>.Ok(sessions, "Sessions fetched successfully."));
        }
    }
}
