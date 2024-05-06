using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace mozgovichok.Models.Organisations
{
    public class Tariff
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public bool HaveCurator { get; set; }
        public int PupilsNumber { get; set; }
        public decimal TariffPrice { get; set; }
        public int TariffDaysDuration { get; set; } 
        public string? Description { get; set; }
    }
}
