using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Organisations;
using mozgovichok.Models;


public class ArchivedPupilsService
{
    private readonly IMongoCollection<Pupil> _archivedPupilsCollection;

    public ArchivedPupilsService(IOptions<UsersDBSettings> usersDBSettings)
    {
        MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

        IMongoDatabase mongoDatabase = client.GetDatabase(usersDBSettings.Value.DatabaseName);

        _archivedPupilsCollection = mongoDatabase.GetCollection<Pupil>(
            usersDBSettings.Value.ArchivedPupilsCollectionName);
    }
    public async Task<List<Pupil>> GetAsync() =>
           await _archivedPupilsCollection.Find(_ => true).ToListAsync();

    public async Task<Pupil?> GetAsync(string id) =>
        await _archivedPupilsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    /// <summary>
    /// возврат выборки подопечных организации
    /// </summary>
    /// <param name="orgId">ИД организации</param>
    /// <returns></returns>
    public async Task<List<Pupil>> GetAsyncForOrg(string orgId)
    {
        List<Pupil> pupils = await _archivedPupilsCollection.Find(x => x.Organisation == orgId).ToListAsync();
        return pupils;
    }


    public async Task CreateAsync(Pupil newPupil) =>
        await _archivedPupilsCollection.InsertOneAsync(newPupil);

    public async Task UpdateAsync(string id, Pupil updatedPupil) =>
        await _archivedPupilsCollection.ReplaceOneAsync(x => x.Id == id, updatedPupil);

    public async Task RemoveAsync(string id) =>
        await _archivedPupilsCollection.DeleteOneAsync(x => x.Id == id);

}

