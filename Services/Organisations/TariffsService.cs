using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Users;
using mozgovichok.Models;
using mozgovichok.Models.Organisations;

namespace mozgovichok.Services.Organisations
{
    public class TariffsService
    {
        private readonly IMongoCollection<Tariff> _tariffsCollection;

        public TariffsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            IMongoDatabase database = client.GetDatabase(usersDBSettings.Value.DatabaseName);

            _tariffsCollection = database.GetCollection<Tariff>("Tariffs");
        }
        public async Task<List<Tariff>> GetAsync() =>
           await _tariffsCollection.Find(_ => true).ToListAsync();

        public async Task<Tariff?> GetAsync(string id) =>
            await _tariffsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Tariff newTariff) =>
            await _tariffsCollection.InsertOneAsync(newTariff);

        public async Task UpdateAsync(string id, Tariff updatedTariff) =>
            await _tariffsCollection.ReplaceOneAsync(x => x.Id == id, updatedTariff);

        public async Task RemoveAsync(string id) =>
            await _tariffsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
