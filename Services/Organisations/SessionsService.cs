using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Courses;
using mozgovichok.Models;
using mozgovichok.Models.Organisations;

namespace mozgovichok.Services.Organisations
{
    public class SessionsService
    {
        private readonly IMongoCollection<Session> _coursesCollection;

        public SessionsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            var mongoClient = new MongoClient(
                usersDBSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                usersDBSettings.Value.DatabaseName);

            _coursesCollection = mongoDatabase.GetCollection<Session>(
                usersDBSettings.Value.SessionsCollectionName);
        }

        public async Task<List<Session>> GetAsync() =>
            await _coursesCollection.Find(_ => true).ToListAsync();

        public async Task<Session?> GetAsync(string id) =>
            await _coursesCollection.Find(x => x.Id.ToString() == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Session newSession) =>
            await _coursesCollection.InsertOneAsync(newSession);

        public async Task UpdateAsync(string id, Session updatedSession) =>
            await _coursesCollection.ReplaceOneAsync(x => x.Id.ToString() == id, updatedSession);

        public async Task RemoveAsync(string id) =>
            await _coursesCollection.DeleteOneAsync(x => x.Id.ToString() == id);
    }
}
