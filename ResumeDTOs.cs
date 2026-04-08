using Microsoft.AspNetCore.Http;

namespace AIResumeAnalyzer.MVC.DTOs
{
    public class UploadResumeRequest
    {
        public IFormFile File { get; set; } = null!;
        public string JobDescription { get; set; } = string.Empty;
    }

    public class ResumeAnalysisResponse
    {
        public string Id { get; set; } = string.Empty;
        public int AtsScore { get; set; }
        public int SkillMatchPercentage { get; set; }
        public List<string> MissingSkills { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string DetectedRole { get; set; } = string.Empty;
    }
}
