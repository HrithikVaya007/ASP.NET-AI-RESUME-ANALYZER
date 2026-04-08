using AIResumeAnalyzer.MVC.Models;
using MongoDB.Driver;

namespace AIResumeAnalyzer.MVC.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.GetCollection<User>("Users");
        }

        public async Task<User?> GetUserByIdAsync(string id) =>
            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<User?> GetUserByPhoneAsync(string phone) =>
            await _users.Find(u => u.Phone == phone).FirstOrDefaultAsync();

        public async Task<List<User>> GetAllUsersAsync() =>
            await _users.Find(_ => true).ToListAsync();

        public async Task CreateUserAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task UpdateUserAsync(string id, User user) =>
            await _users.ReplaceOneAsync(u => u.Id == id, user);

        public async Task DeleteUserAsync(string id) =>
            await _users.DeleteOneAsync(u => u.Id == id);
    }
}
