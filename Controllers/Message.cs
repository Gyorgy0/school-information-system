namespace SchoolAPI.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string? MessageText { get; set; }
        public DateTime Timestamp { get; set; }
    }
}