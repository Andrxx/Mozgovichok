using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace mozgovichok.Models.Organisations
{
    public class Balance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Comment { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public DateTime TariffExpiration { get; set; }
        public decimal? Value { get; set; }
        public string? Currency { get; set; }
    }
}
