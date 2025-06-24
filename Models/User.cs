namespace SchoolAPI.Models
{
    public class User
    {
        public long UserID { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
    }
}
