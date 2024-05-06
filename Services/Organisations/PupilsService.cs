using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models;
using mozgovichok.Models.Organisations;
using mozgovichok.Models.Users;

namespace mozgovichok.Services.Organisations
{
    public class PupilsService
    {
        private readonly IMongoCollection<Pupil> _pupilsCollection;

        public PupilsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            IMongoDatabase mongoDatabase = client.GetDatabase(usersDBSettings.Value.DatabaseName);

            _pupilsCollection = mongoDatabase.GetCollection<Pupil>(
                usersDBSettings.Value.PupilsCollectionName);

            //_pupilsCollection = database.GetCollection<Pupil>("Pupils");
        }
        public async Task<List<Pupil>> GetAsync() =>
           await _pupilsCollection.Find(_ => true).ToListAsync();

        public async Task<Pupil?> GetAsync(string id) =>
            await _pupilsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        /// <summary>
        /// возврат выборки подопечных организации
        /// </summary>
        /// <param name="orgId">ИД организации</param>
        /// <returns></returns>
        public async Task<List<Pupil>> GetAsyncForOrg(string orgId)
        {
            List<Pupil> pupils = await _pupilsCollection.Find(x => x.Organisation == orgId).ToListAsync(); //.AsQueryable().Where(p => p.Organisation == orgId);
            return pupils;
        }
            

        public async Task CreateAsync(Pupil newPupil) =>
            await _pupilsCollection.InsertOneAsync(newPupil);

        public async Task UpdateAsync(string id, Pupil updatedPupil)
        {
            await _pupilsCollection.ReplaceOneAsync(x => x.Id == id, updatedPupil);
        }

        public async Task RemoveAsync(string id) =>
            await _pupilsCollection.DeleteOneAsync(x => x.Id == id);



    }
}
