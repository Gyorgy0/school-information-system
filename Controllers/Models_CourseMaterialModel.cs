namespace SchoolAPI.Models
{
    public class CourseMaterialModel
    {
        public int MaterialID { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}