﻿using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Users;
using mozgovichok.Models;

namespace mozgovichok.Services.Users
{
    public class AdminsService
    {
        private readonly IMongoCollection<Admin> _adminsCollection;

        public AdminsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            IMongoDatabase database = client.GetDatabase(usersDBSettings.Value.DatabaseName);

            _adminsCollection = database.GetCollection<Admin>("Admins");
        }
        public async Task<List<Admin>> GetAsync() =>
           await _adminsCollection.Find(_ => true).ToListAsync();

        public async Task<Admin?> GetAsync(string id) =>
            await _adminsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Admin newAdmin) =>
            await _adminsCollection.InsertOneAsync(newAdmin);

        public async Task UpdateAsync(string id, Admin updatedAdmin) =>
            await _adminsCollection.ReplaceOneAsync(x => x.Id == id, updatedAdmin);

        public async Task RemoveAsync(string id) =>
            await _adminsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
