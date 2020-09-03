namespace Vanjaro.Common.Data.Entities
{
    public class SmtpServer
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Authentication { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool SSL { get; set; }
    }
}