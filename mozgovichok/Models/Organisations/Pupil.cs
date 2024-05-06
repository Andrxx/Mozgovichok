using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using mozgovichok.Models.Courses;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace mozgovichok.Models.Organisations
{
    public class Pupil/* : ICloneable*/
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        //public DateOnly? BirthDate { get; set; }
        public string? Interests { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string? LeadHand { get; set; }
        public PupilRecomendations? Recomendations { get; set; }
        public Course? ActiveCourse { get; set; }
        public List<Course>? PassedCourses { get; set; }
        public string? Organisation {  get; set; }
        [NotMapped]
        public bool? isExerciseReady { get; set; }


        public Pupil Clone() 
        {
            return (Pupil)this.MemberwiseClone();
        }
    }

    public enum PupilRecomendations
    {
        None = 0,
        Decrease = 1,
        Invariably = 2,
        Increase = 3,
    }

    

}
