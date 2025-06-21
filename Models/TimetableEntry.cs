namespace SchoolAPI.Models
{
    public class TimetableEntry
    {
        public Int64 TimetableID { get; set; }
        public string? Day { get; set; }
        public string? Hour { get; set; }
        public string? Subject { get; set; }
        public string? Classroom { get; set; }
        public Int64 TeacherID { get; set; }
    }
}
