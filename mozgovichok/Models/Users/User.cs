using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;


namespace mozgovichok.Models.Users
{
    public class User /*: MongoIdentityUser<Guid>*/
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? SurName { get; set; }
        public string? Phone {  get; set; }
        public string? Email { get; set; }
        public string Role { get; set; }
        public string? Password { get; set; }
        public string? OrganisationId { get; set; }
        public string? Goal { get; set; }


    }
}
