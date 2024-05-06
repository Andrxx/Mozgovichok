using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using mozgovichok.Models.Courses.Statistics;

namespace mozgovichok.Models.Courses
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? isFinished { get; set; } = false;
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
        public CourseStatistics? Statistics { get; set; }
    }
}
