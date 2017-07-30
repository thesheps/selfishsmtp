namespace SelfishSmtp
{
    public class MailMessage
    {
        public string From { get; set; }
        public string Date { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
    }
}