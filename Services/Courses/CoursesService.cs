using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Courses;
using mozgovichok.Models;

namespace mozgovichok.Services.Courses
{
    public class CoursesService
    {
        private readonly IMongoCollection<Course> _coursesCollection;

        public CoursesService(IOptions<UsersDBSettings> usersDBSettings)
        {
            var mongoClient = new MongoClient(
                usersDBSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                usersDBSettings.Value.DatabaseName);

            _coursesCollection = mongoDatabase.GetCollection<Course>(
                usersDBSettings.Value.CoursesCollectionName);
        }

        public async Task<List<Course>> GetAsync() =>
            await _coursesCollection.Find(_ => true).ToListAsync();

        public async Task<Course?> GetAsync(string id) =>
            await _coursesCollection.Find(x => x.Id.ToString() == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Course newCourse)
        {
            await _coursesCollection.InsertOneAsync(newCourse);
        }
            

        public async Task UpdateAsync(string id, Course updatedCourse) =>
            await _coursesCollection.ReplaceOneAsync(x => x.Id.ToString() == id, updatedCourse);

        public async Task RemoveAsync(string id) =>
            await _coursesCollection.DeleteOneAsync(x => x.Id.ToString() == id);
    }
}
