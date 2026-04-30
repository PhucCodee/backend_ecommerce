namespace ECommerce.Infrastructure.Common
{
    public class EmailSettings
    {
        public static string SectionName => "EmailSettings";
        public string? Provider { get; set; }
        public string? ApiKey { get; set; }
        public string? FromName { get; set; }
        public string? FromAddress { get; set; }

        // Add SMTP properties if you want to support SMTP as well
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUser { get; set; }
        public string? SmtpPass { get; set; }
    }
}
