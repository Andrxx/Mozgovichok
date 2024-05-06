using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models;
using mozgovichok.Models.Users;

namespace mozgovichok.Services.Users
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UsersService(IOptions<UsersDBSettings> usersDBSettings)
        {
            var mongoClient = new MongoClient(
                usersDBSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                usersDBSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                usersDBSettings.Value.UsersCollectionName);
        }

        public async Task<List<User>> GetAsync() =>
            await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetAsync(string id) =>
            await _usersCollection.Find(x => x.Id.ToString() == id).FirstOrDefaultAsync();

        public async Task CreateAsync(User newUser) =>
            await _usersCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, User updatedUser) =>
            await _usersCollection.ReplaceOneAsync(x => x.Id.ToString() == id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id.ToString() == id);

        /// <summary>
        /// проверяем базу на наличие одинаковых email, true если запись найдена (дублирование)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task< bool> FindMailInBase(string email)
        {
            if (await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync() is null)  return false; 
            return true;
        }
    }
}
