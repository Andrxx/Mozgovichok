using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace mozgovichok.Models.DTO
{
    public class SignupModel
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; }
        public string? Role { get; set; }
        public string Password { get; set; }
        public string? OrganisationId { get; set; }
        public string? Goal { get; set; }
        public string? City { get; set; }
        public string? OrganizationName { get; set; }
        public string? Inn { get; set; }
        public string? Requisites { get; set; }
    }
}
