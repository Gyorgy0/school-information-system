namespace SchoolAPI.Models
{
    public class CourseModel
    {
        public int CourseID { get; set; }
        public string? Name { get; set; }
        public int TeacherID { get; set; }
        public bool Visible { get; set; }
    }
}
