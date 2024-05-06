using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mozgovichok.Models.Users;
using mozgovichok.Models;
using mozgovichok.Models.Organisations;
using MongoDB.Bson;
using System.Collections.Generic;
using mozgovichok.Infrastructure;

namespace mozgovichok.Services.Organisations
{
    public class OrganisationsService
    {
        private readonly IMongoCollection<Organisation> _organisationCollection;

        public OrganisationsService(IOptions<UsersDBSettings> usersDBSettings)
        {
            MongoClient client = new MongoClient(usersDBSettings.Value.ConnectionString);

            IMongoDatabase database = client.GetDatabase(usersDBSettings.Value.DatabaseName);

            _organisationCollection = database.GetCollection<Organisation>("Organisation");
        }
        public async Task<List<Organisation>> GetAsync() =>
            await _organisationCollection.Find(_ => true).ToListAsync();

        /// <summary>
        /// возврат организации или null если id null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Organisation?> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            return await _organisationCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Organisation newOrganisation) =>
            await _organisationCollection.InsertOneAsync(newOrganisation);

        public async Task UpdateAsync(string id, Organisation updatedOrganisation) =>
            await _organisationCollection.ReplaceOneAsync(x => x.Id == id, updatedOrganisation);

        public async Task RemoveAsync(string id) =>
             await _organisationCollection.DeleteOneAsync(x => x.Id == id);


        //работа с платежами

        /// <summary>
        /// возвращем список платежей организции по id, null если не найдена организация, пустой список, если нет платежей
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Payment>> GetPaymentsAsync(string id)
        {
            var organisation = await _organisationCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            List<Payment> orgPayments = new();
            if(organisation is null)
            {
                return null;
            }
            if (organisation.Payments != null)
            {
                orgPayments = organisation.Payments;
            }
            return orgPayments;
        }

        /// <summary>
        /// добавляем платеж в список организции, возврат null при ошибке 
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public async Task<Payment> CreatePaymentAsync(string orgId, Payment payment)
        {
            var organisation = await _organisationCollection.Find(x => x.Id == orgId).FirstOrDefaultAsync();
            if (organisation is null || payment is null)
            {
                return null;
            }
            if(string.IsNullOrEmpty(payment.Id))
            {
                payment.Id = ObjectId.GenerateNewId().ToString();
            }


            Balance balance = BalanceHandler.Count(payment, organisation.Tariff);
            organisation.Payments ??= new List<Payment>();
            organisation.Payments.Add(payment);
            return payment;
        }
    }
}
