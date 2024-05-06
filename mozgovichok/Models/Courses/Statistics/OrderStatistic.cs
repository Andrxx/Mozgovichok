namespace mozgovichok.Models.Courses.Statistics
{
    public class OrderStatistic
    {
        public int? OrderErrors { get; set; }
        public float? OrderTime { get; set; }
        public float? LatencyTime { get; set; }
        public float? AverageLatencyTime { get; set; }
        public float? VoiceLatencyTime { get; set; }
        public int? ImpulseAction { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set;}
    }
}
