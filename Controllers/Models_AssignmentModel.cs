namespace SchoolAPI.Models
{
    public class AssignmentModel
    {
        public int AssignmentID { get; set; }
        public int CourseID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }
}