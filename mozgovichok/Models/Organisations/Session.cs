using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using mozgovichok.Models.Courses;

namespace mozgovichok.Models.Organisations
{
    public class Session
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public bool IsReady { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? OrganisationStarted { get; set; }
        public Pupil? ExaminedPupil { get; set; }
        //public Course? CurrentCousre { get; set; }

    }
}
