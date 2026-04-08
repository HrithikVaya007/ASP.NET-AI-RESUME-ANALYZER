using AIResumeAnalyzer.MVC.DTOs;
using AIResumeAnalyzer.MVC.Helpers;
using AIResumeAnalyzer.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionsController : ControllerBase
    {
        private readonly ResumeService _resumeService;

        public QuestionsController(ResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateQuestionsRequest request)
        {
            var pdfPath = await _resumeService.GenerateInterviewQuestionsPdfAsync(request.ResumeId);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fullUrl = $"{baseUrl}{pdfPath}";

            return Ok(ApiResponse<GenerateQuestionsResponse>.Ok(new GenerateQuestionsResponse { PdfUrl = fullUrl }, "Questions generated successfully."));
        }
    }
}
