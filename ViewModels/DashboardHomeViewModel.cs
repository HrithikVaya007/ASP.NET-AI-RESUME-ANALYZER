namespace AIResumeAnalyzer.MVC.Models.ViewModels
{
    public class DashboardHomeViewModel
    {
        public User User { get; set; } = new();
        
        // Latest Resume Stats
        public int LatestAtsScore { get; set; }
        public int LatestSkillMatch { get; set; }
        
        // Latest Interview Stats
        public int LatestReadinessScore { get; set; }
        public int TotalSessions { get; set; }
        
        // Flags to show if data exists
        public bool HasResumeHistory => LatestAtsScore > 0 || LatestSkillMatch > 0;
        public bool HasInterviewHistory => TotalSessions > 0;
    }
}
