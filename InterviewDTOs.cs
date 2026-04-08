namespace AIResumeAnalyzer.MVC.DTOs
{
    public class GenerateQuestionsRequest
    {
        public string ResumeId { get; set; } = string.Empty;
    }

    public class GenerateQuestionsResponse
    {
        public string PdfUrl { get; set; } = string.Empty; // URL to access generated PDF
    }
    
    public class StartInterviewRequest
    {
        public string ResumeId { get; set; } = string.Empty;
    }

    public class NextQuestionRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public string PreviousAnswer { get; set; } = string.Empty;
    }

    public class NextQuestionResponse
    {
        public string NextQuestion { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
    }

    public class InterviewFeedbackRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }

    public class InterviewFeedbackResponse
    {
        public int FinalScore { get; set; }
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }
}
