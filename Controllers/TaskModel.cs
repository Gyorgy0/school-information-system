<<<<<<< HEAD
namespace SchoolAPI.Models.Task
{
    public class TaskModel
    {
        public int TaskID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int SubjectID { get; set; }
        public string? SubjectName { get; set; }
        public int TeacherID { get; set; }
        public DateTime DueDate { get; set; }
    }
=======
namespace SchoolAPI.Models.Task
{
    public class TaskModel
    {
        public int TaskID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int SubjectID { get; set; }
        public string? SubjectName { get; set; }
        public int TeacherID { get; set; }
        public DateTime DueDate { get; set; }
    }
>>>>>>> kovacs-mark
}