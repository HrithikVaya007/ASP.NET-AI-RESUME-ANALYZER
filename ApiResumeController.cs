using AIResumeAnalyzer.MVC.DTOs;
using AIResumeAnalyzer.MVC.Helpers;
using AIResumeAnalyzer.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Route("api/resume")]
    [ApiController]
    [Authorize]
    public class ApiResumeController : ControllerBase
    {
        private readonly ResumeService _resumeService;

        public ApiResumeController(ResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadResumeRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Error("Invalid file."));
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = System.IO.Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(ApiResponse<object>.Error("Only PDF and DOC/DOCX files are permitted."));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            try
            {
                var response = await _resumeService.ProcessResumeAsync(request.File, userId, request.JobDescription);
                return Ok(ApiResponse<ResumeAnalysisResponse>.Ok(response, "Resume analyzed successfully."));
            }
            catch (Exception ex) when (ex.Message == "INVALID_RESUME_DETECTED")
            {
                return BadRequest(ApiResponse<object>.Error("The uploaded document does not appear to be a valid resume."));
            }
            catch (Exception ex)
            {
                // Log the exception locally if possible, but return message for debugging
                return BadRequest(ApiResponse<object>.Error($"Analysis Error: {ex.Message}"));
            }
        }

        [HttpGet("report/{id}")]
        public async Task<IActionResult> DownloadReport(string id)
        {
            try
            {
                var pdfUrl = await _resumeService.GenerateAnalysisReportAsync(id);
                return Ok(ApiResponse<object>.Ok(new { url = pdfUrl }, "Report generated."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("compare")]
        public async Task<IActionResult> Compare([FromForm] CompareResumesRequest request)
        {
            if (request.FileA == null || request.FileB == null)
            {
                return BadRequest(ApiResponse<object>.Error("Both resumes must be uploaded for comparison."));
            }

            try
            {
                var response = await _resumeService.CompareResumesAsync(request.FileA, request.FileB, request.JobDescription);
                return Ok(ApiResponse<object>.Ok(response, "Comparison complete."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }
    }

    public class CompareResumesRequest
    {
        public IFormFile? FileA { get; set; }
        public IFormFile? FileB { get; set; }
        public string JobDescription { get; set; } = string.Empty;
    }
}
