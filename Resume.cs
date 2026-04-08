using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AIResumeAnalyzer.MVC.Models
{
    public class Resume
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string ExtractedText { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public int AtsScore { get; set; }

        public int SkillMatchPercentage { get; set; }

        public List<string> MissingSkills { get; set; } = new();

        public List<string> Suggestions { get; set; } = new();

        public string DetectedRole { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
