namespace SchoolAPI.Models
{
    public class TimetableEntry
    {
        public string? Day { get; set; }

        public long Hour { get; set; }
        public string? Subject { get; set; }
        public string? Classroom { get; set; }
        public long TeacherID { get; set; }
    }
}
