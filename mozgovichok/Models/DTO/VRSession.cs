using mozgovichok.Models.Courses;

namespace mozgovichok.Models.DTO
{
    public class VRSession
    {
        public string? SessionId { get; set; }
        public string? PupilName { get; set; }
        public string? PupilId { get; set; }
        public int? PupilAge {  get; set; }
        public Exercise? Exercise { get; set; }
    }
}
