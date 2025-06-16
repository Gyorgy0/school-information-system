namespace SchoolAPI.Models
{
    public class CourseMaterialModel
    {
        public int MaterialID { get; set; }
        public int CourseID { get; set; }
<<<<<<< HEAD
        public string Title { get; set; }
        public string Url { get; set; }
=======
        public string? Title { get; set; }
        public string? Url { get; set; }
>>>>>>> kovacs-mark
        public DateTime UploadedAt { get; set; }
    }
}