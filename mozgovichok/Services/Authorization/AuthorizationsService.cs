using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Users;
using mozgovichok.Models;

namespace mozgovichok.Services.Autorization
{
    public class AuthorizationsService
    {
        //private readonly IMongoCollection<User> _usersCollection;

        public AuthorizationsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            //MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            //IMongoDatabase database = client.GetDatabase(usersDBSettings.Value.DatabaseName);

            //_usersCollection = database.GetCollection<User>("Users");
        }
        //public async Task<List<User>> GetAsync() =>
        //   await _usersCollection.Find(_ => true).ToListAsync();

        //public async Task<User?> GetAsync(string id) =>
        //    await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    }
}
