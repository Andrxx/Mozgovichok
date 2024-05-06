using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace mozgovichok.Models.Organisations
{
    public class Organisation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Name { get; set; }
        public string? Comment { get; set; }
        public string? City { get; set; }
        public List<string> ActivePupils { get; set; } = new List<string>();        //храним ид ассоциированых подопечных
        public List<string> ActiveUsers { get; set; } = new List<string>();        //храним ид ассоциированых специалистов
        public List<string> ArchivedPupils { get; set; } = new List<string>();
        public List<Session>? Sessions { get; set; } = new List<Session>();
         public Tariff? Tariff { get; set; }
        public List<Payment>? Payments { get; set; } = new List<Payment>();
        public Balance? Balance { get; set; }
        public string? Inn { get; set; }   
        public string? Requisites { get; set;}
       
    }
}
