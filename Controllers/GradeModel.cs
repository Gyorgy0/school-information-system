namespace SchoolAPI.Models.Grade
{
    public class GradeModel
    {
        public int StudentID { get; set; }
        public string? StudentName { get; set; }
        public int TaskID { get; set; }
        public string? TaskTitle { get; set; }
        public float StudentGrade { get; set; }
        public DateTime Date { get; set; }
    }
}