using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Users;
using mozgovichok.Models;
using mozgovichok.Models.Courses;

namespace mozgovichok.Services.Courses
{
    public class OrdersService
    {
        private readonly IMongoCollection<Order> _ordersCollection;

        public OrdersService(IOptions<UsersDBSettings> usersDBSettings)
        {
            var mongoClient = new MongoClient(
                usersDBSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                usersDBSettings.Value.DatabaseName);

            _ordersCollection = mongoDatabase.GetCollection<Order>(
                usersDBSettings.Value.OrdersCollectionName);
        }

        public async Task<List<Order>> GetAsync() =>
            await _ordersCollection.Find(_ => true).ToListAsync();

        public async Task<Order?> GetAsync(string id) =>
            await _ordersCollection.Find(x => x.Id.ToString() == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Order newOrder) =>
            await _ordersCollection.InsertOneAsync(newOrder);

        public async Task UpdateAsync(string id, Order updatedOrder) =>
            await _ordersCollection.ReplaceOneAsync(x => x.Id.ToString() == id, updatedOrder);

        public async Task RemoveAsync(string id) =>
            await _ordersCollection.DeleteOneAsync(x => x.Id.ToString() == id);
    }
}
