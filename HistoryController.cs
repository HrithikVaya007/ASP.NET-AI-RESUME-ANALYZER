using AIResumeAnalyzer.MVC.Helpers;
using AIResumeAnalyzer.MVC.Models;
using AIResumeAnalyzer.MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly ResumeRepository _resumeRepository;
        private readonly InterviewSessionRepository _sessionRepository;

        public HistoryController(ResumeRepository resumeRepository, InterviewSessionRepository sessionRepository)
        {
            _resumeRepository = resumeRepository;
            _sessionRepository = sessionRepository;
        }

        [HttpGet("resumes")]
        public async Task<IActionResult> GetResumes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var resumes = await _resumeRepository.GetResumesByUserIdAsync(userId);
            
            // returning only necessary projection could be better, but returning model directly for brevity here
            return Ok(ApiResponse<List<Resume>>.Ok(resumes, "Resumes fetched successfully."));
        }

        [HttpGet("interviews")]
        public async Task<IActionResult> GetInterviews()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var sessions = await _sessionRepository.GetSessionsByUserIdAsync(userId);

            return Ok(ApiResponse<List<InterviewSession>>.Ok(sessions, "Sessions fetched successfully."));
        }

        [HttpDelete("resume/{id}")]
        public async Task<IActionResult> DeleteResume(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var resume = await _resumeRepository.GetResumeByIdAsync(id);
            if (resume == null || resume.UserId != userId) return NotFound();

            await _resumeRepository.DeleteResumeAsync(id);
            return Ok(ApiResponse<object>.Ok(new { }, "Resume deleted successfully."));
        }

        [HttpDelete("interview/{id}")]
        public async Task<IActionResult> DeleteInterview(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var session = await _sessionRepository.GetSessionByIdAsync(id);
            if (session == null || session.UserId != userId) return NotFound();

            await _sessionRepository.DeleteSessionAsync(id);
            return Ok(ApiResponse<object>.Ok(new { }, "Interview session deleted successfully."));
        }
    }
}
