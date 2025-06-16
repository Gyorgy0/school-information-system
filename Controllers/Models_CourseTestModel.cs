namespace SchoolAPI.Models
{
    public class CourseTestModel
    {
        public int TestID { get; set; }
        public int CourseID { get; set; }
<<<<<<< HEAD
        public string Title { get; set; }
        public string Description { get; set; }
=======
        public string? Title { get; set; }
        public string? Description { get; set; }
>>>>>>> kovacs-mark
        public DateTime? DueDate { get; set; }
    }
}