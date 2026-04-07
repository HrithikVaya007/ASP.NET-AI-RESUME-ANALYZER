using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AIResumeAnalyzer.MVC.Models
{
    public class InterviewSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string ResumeId { get; set; } = string.Empty;

        public string DetectedRole { get; set; } = string.Empty;

        public List<InterviewQnA> QnAs { get; set; } = new();

        public int FinalScore { get; set; }

        public List<string> Strengths { get; set; } = new();

        public List<string> Weaknesses { get; set; } = new();

        public List<string> Suggestions { get; set; } = new();

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }

    public class InterviewQnA
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime AskedAt { get; set; } = DateTime.UtcNow;
    }
}
