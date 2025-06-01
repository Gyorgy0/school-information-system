namespace SchoolAPI.Models
{
    public class Grade
    {
        public int GradeID { get; set; }
        public int UserID { get; set; }
        public string? Subject { get; set; }
        public int GradeValue { get; set; }
        public DateTime Date { get; set; }
    }
}