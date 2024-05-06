namespace mozgovichok.Models
{
    public class UsersDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get;  set; } = null!;
        public string OrdersCollectionName { get;  set; } = null!;
        public string ExercisesCollectionName { get; set; } = null!;
        public string CoursesCollectionName { get; set; } = null!;
        public string SessionsCollectionName { get;  set; } = null!;
        public string NewsCollectionName { get; set; } = null!;
        public string PupilsCollectionName { get; set; } = null!;
        public string ArchivedPupilsCollectionName { get; set; } = null!;
    }
}
