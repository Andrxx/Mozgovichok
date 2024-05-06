using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace mozgovichok.Models.Organisations
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Comment { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? Currency {  get; set; }
        public string? Order { get; set; }
    }
}
