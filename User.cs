using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AIResumeAnalyzer.MVC.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User"; // Can be 'Admin' or 'User'

        public string? Phone { get; set; }
        public string? SmsResetCode { get; set; }
        public DateTime? ResetCodeExpiration { get; set; }

        public string? Location { get; set; }

        public UserSettings Settings { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserSettings
    {
        public bool EmailNotifications { get; set; } = true;
        public bool WeeklyReports { get; set; } = false;
        public bool InterviewReminders { get; set; } = true;
        public string DefaultRole { get; set; } = "Software Engineer";
        public string AiStrictness { get; set; } = "balanced"; // forgiving, balanced, strict
    }
}
