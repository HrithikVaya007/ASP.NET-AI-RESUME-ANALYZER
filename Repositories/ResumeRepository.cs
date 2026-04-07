using AIResumeAnalyzer.MVC.Models;
using MongoDB.Driver;

namespace AIResumeAnalyzer.MVC.Repositories
{
    public class ResumeRepository
    {
        private readonly IMongoCollection<Resume> _resumes;

        public ResumeRepository(MongoDbContext context)
        {
            _resumes = context.GetCollection<Resume>("Resumes");
        }

        public async Task<Resume?> GetResumeByIdAsync(string id) =>
            await _resumes.Find(r => r.Id == id).FirstOrDefaultAsync();

        public async Task<List<Resume>> GetResumesByUserIdAsync(string userId) =>
            await _resumes.Find(r => r.UserId == userId).ToListAsync();

        public async Task<Resume?> GetLatestResumeByUserIdAsync(string userId) =>
            await _resumes.Find(r => r.UserId == userId)
                          .SortByDescending(r => r.UploadedAt)
                          .FirstOrDefaultAsync();

        public async Task<List<Resume>> GetAllResumesAsync() =>
            await _resumes.Find(_ => true).ToListAsync();

        public async Task CreateResumeAsync(Resume resume) =>
            await _resumes.InsertOneAsync(resume);
            
        public async Task UpdateResumeAsync(string id, Resume resume) =>
            await _resumes.ReplaceOneAsync(r => r.Id == id, resume);

        public async Task DeleteResumeAsync(string id) =>
            await _resumes.DeleteOneAsync(r => r.Id == id);
    }
}
