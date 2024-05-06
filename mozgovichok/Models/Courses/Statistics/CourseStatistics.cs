namespace mozgovichok.Models.Courses.Statistics

{
    public class CourseStatistics
    {
        string? Id { get; set; }
        public string? Name { get; set; }
        //выведено для отладки, включить и обработать на сервере
        //public TimeSpan? TotalTime { get; set; }y
        //public DateTime? StartTime { get; set; }
        //public DateTime? EndTime { get; set;}
        public string? Errors { get; set; }
    }
}
