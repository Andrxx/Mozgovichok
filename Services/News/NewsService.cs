using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Organisations;
using mozgovichok.Models;
using mozgovichok.Models.News;
using mozgovichok.Models.Users;

namespace mozgovichok.Services.News
{
    public class NewsService
    {
        private readonly IMongoCollection<NewsModel> _newsCollection;

        public NewsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            var mongoDatabase = client.GetDatabase(
                usersDBSettings.Value.DatabaseName);

            _newsCollection = mongoDatabase.GetCollection<NewsModel>(
                usersDBSettings.Value.NewsCollectionName);
        }
        public async Task<List<NewsModel>> GetAsync() =>
           await _newsCollection.Find(_ => true).ToListAsync();

        public async Task<NewsModel?> GetAsync(string id) =>
            await _newsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(NewsModel newsModel) =>
            await _newsCollection.InsertOneAsync(newsModel);

        public async Task UpdateAsync(string id, NewsModel updatedNews) =>
            await _newsCollection.ReplaceOneAsync(x => x.Id == id, updatedNews);

        public async Task RemoveAsync(string id) =>
            await _newsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
