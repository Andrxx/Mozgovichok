using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Courses;
using mozgovichok.Models;

namespace mozgovichok.Services.Courses
{
    public class ExercisesService
    {
        private readonly IMongoCollection<Exercise> _exercisesCollection;

        public ExercisesService(IOptions<UsersDBSettings> usersDBSettings)
        {
            var mongoClient = new MongoClient(
                usersDBSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                usersDBSettings.Value.DatabaseName);

            _exercisesCollection = mongoDatabase.GetCollection<Exercise>(
                usersDBSettings.Value.ExercisesCollectionName);
        }

        public async Task<List<Exercise>> GetAsync() =>
            await _exercisesCollection.Find(_ => true).ToListAsync();

        public async Task<Exercise?> GetAsync(string id) =>
            await _exercisesCollection.Find(x => x.Id.ToString() == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Exercise newExercise) =>
            await _exercisesCollection.InsertOneAsync(newExercise);

        public async Task UpdateAsync(string id, Exercise updatedExercise) =>
            await _exercisesCollection.ReplaceOneAsync(x => x.Id.ToString() == id, updatedExercise);

        public async Task RemoveAsync(string id) =>
            await _exercisesCollection.DeleteOneAsync(x => x.Id.ToString() == id);
    }
}
