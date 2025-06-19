namespace SchoolAPI.Models.Timetable
{
    public class Timetable
    {
        public int AbsenceID { get; set; }
        public int TimetableID { get; set; }
        public int StudentID { get; set; }
        public DateTime Date { get; set; }
        public int EventID { get; set; }
        public string? EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string? Description { get; set; }
    }
}