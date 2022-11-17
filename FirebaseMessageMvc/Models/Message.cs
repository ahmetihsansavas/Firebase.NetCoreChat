namespace FirebaseMessageMvc.Models
{
    public class Message
    {
        public string MessageId { get; set; }
        public string Data { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
