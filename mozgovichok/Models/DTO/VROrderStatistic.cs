using mozgovichok.Models.Courses;

namespace mozgovichok.Models.DTO
{
    public class VROrderStatistic
    {
        public string? PupilId { get; set; }
        public string? ExerciseId { get; set; }
        public string? OrderId { get; set; }
        public bool isFinished { get; set; }
        public int? OrderErrors { get; set; }
        public float? OrderTime { get; set; }
        public float? LatencyTime { get; set; }
        public float? AverageLatencyTime { get; set; }
        public float? VoiceLatencyTime { get; set; }
        public int? ImpulseAction { get; set; }

    }
}
