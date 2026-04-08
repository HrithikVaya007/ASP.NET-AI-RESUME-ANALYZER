using AIResumeAnalyzer.MVC.Models;
using MongoDB.Driver;

namespace AIResumeAnalyzer.MVC.Repositories
{
    public class InterviewSessionRepository
    {
        private readonly IMongoCollection<InterviewSession> _sessions;

        public InterviewSessionRepository(MongoDbContext context)
        {
            _sessions = context.GetCollection<InterviewSession>("InterviewSessions");
        }

        public async Task<InterviewSession?> GetSessionByIdAsync(string id) =>
            await _sessions.Find(s => s.Id == id).FirstOrDefaultAsync();

        public async Task<List<InterviewSession>> GetSessionsByUserIdAsync(string userId) =>
            await _sessions.Find(s => s.UserId == userId).ToListAsync();

        public async Task<InterviewSession?> GetLatestSessionByUserIdAsync(string userId) =>
            await _sessions.Find(s => s.UserId == userId)
                           .SortByDescending(s => s.StartedAt)
                           .FirstOrDefaultAsync();

        public async Task<long> GetTotalSessionsCountByUserIdAsync(string userId) =>
            await _sessions.CountDocumentsAsync(s => s.UserId == userId);

        public async Task<List<InterviewSession>> GetAllSessionsAsync() =>
            await _sessions.Find(_ => true).ToListAsync();

        public async Task CreateSessionAsync(InterviewSession session) =>
            await _sessions.InsertOneAsync(session);

        public async Task UpdateSessionAsync(string id, InterviewSession session) =>
            await _sessions.ReplaceOneAsync(s => s.Id == id, session);

        public async Task DeleteSessionAsync(string id) =>
            await _sessions.DeleteOneAsync(s => s.Id == id);
    }
}
