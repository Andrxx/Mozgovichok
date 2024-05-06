using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace mozgovichok.Models.News
{
    public class NewsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Header { get; set; }
        public string? Content { get; set; }
        public string? HeaderImage { get; set; }
    }
}
