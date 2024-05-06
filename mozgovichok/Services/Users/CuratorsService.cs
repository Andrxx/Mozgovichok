using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Users;
using mozgovichok.Models;

namespace mozgovichok.Services.Users
{
    public class CuratorsService
    {
        private readonly IMongoCollection<Curator> _curatorsCollection;

        public CuratorsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            IMongoDatabase database = client.GetDatabase(usersDBSettings.Value.DatabaseName);

            _curatorsCollection = database.GetCollection<Curator>("Curators");
        }
        public async Task<List<Curator>> GetAsync() =>
           await _curatorsCollection.Find(_ => true).ToListAsync();

        public async Task<Curator?> GetAsync(string id) =>
            await _curatorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Curator newCurator) =>
            await _curatorsCollection.InsertOneAsync(newCurator);

        public async Task UpdateAsync(string id, Curator updatedCurator) =>
            await _curatorsCollection.ReplaceOneAsync(x => x.Id == id, updatedCurator);

        public async Task RemoveAsync(string id) =>
            await _curatorsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
