namespace SchoolAPI.Models
{
    public class TimetableEntry
    {
        public int TimetableID { get; set; }
        public string? Day { get; set; }
        public string? Hour { get; set; }
        public string? Subject { get; set; }
        public string? Room { get; set; }
        public int TeacherID { get; set; }
        public int ClassID { get; set; }
    }
}