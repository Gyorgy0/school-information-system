namespace SchoolAPI.Models
{
    public class Homework
    {
        public int HomeworkID { get; set; }
        public int UserID { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
    }
}