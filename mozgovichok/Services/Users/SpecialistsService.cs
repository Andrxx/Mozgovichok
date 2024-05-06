using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Users;
using mozgovichok.Models;

namespace mozgovichok.Services.Users
{
    public class SpecialistsService
    {
        private readonly IMongoCollection<Specialist> _specialistsCollection;

        public SpecialistsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            IMongoDatabase database = client.GetDatabase(usersDBSettings.Value.DatabaseName);

            _specialistsCollection = database.GetCollection<Specialist>("Specialists");
        }

        public async Task<List<Specialist>> GetAsync() =>
          await _specialistsCollection.Find(_ => true).ToListAsync();

        public async Task<Specialist?> GetAsync(string id) =>
            await _specialistsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Specialist newSpecialist) =>
            await _specialistsCollection.InsertOneAsync(newSpecialist);

        public async Task UpdateAsync(string id, Specialist updatedSpecialist) =>
            await _specialistsCollection.ReplaceOneAsync(x => x.Id == id, updatedSpecialist);

        public async Task RemoveAsync(string id) =>
            await _specialistsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
