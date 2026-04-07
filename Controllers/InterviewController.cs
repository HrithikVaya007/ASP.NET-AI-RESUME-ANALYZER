using AIResumeAnalyzer.MVC.DTOs;
using AIResumeAnalyzer.MVC.Helpers;
using AIResumeAnalyzer.MVC.Models;
using AIResumeAnalyzer.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InterviewController : ControllerBase
    {
        private readonly InterviewService _interviewService;

        public InterviewController(InterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartInterviewRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var session = await _interviewService.StartSessionAsync(request.ResumeId, userId);

            return Ok(ApiResponse<InterviewSession>.Ok(session, "Session started successfully."));
        }

        [HttpPost("next")]
        public async Task<IActionResult> Next([FromBody] NextQuestionRequest request)
        {
            var response = await _interviewService.GetNextQuestionAsync(request.SessionId, request.PreviousAnswer);

            return Ok(ApiResponse<NextQuestionResponse>.Ok(response, "Next question generated."));
        }

        [HttpPost("feedback")]
        public async Task<IActionResult> Feedback([FromBody] InterviewFeedbackRequest request)
        {
            var response = await _interviewService.CompleteSessionAsync(request.SessionId);

            return Ok(ApiResponse<InterviewFeedbackResponse>.Ok(response, "Feedback generated successfully."));
        }
    }
}
