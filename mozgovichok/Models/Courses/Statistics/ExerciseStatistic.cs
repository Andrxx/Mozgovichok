namespace mozgovichok.Models.Courses.Statistics
{
    public class ExerciseStatistic
    {
        public int? ExerciseErrors { get; set; }
        public float? ExerciseTime { get; set; }
        public float? LatencyTime { get; set; }
        public float? AverageLatencyTime { get; set; }
        public float? VoiceLatencyTime { get; set; }
        public int? ImpulseAction { get; set; }
    }
}
