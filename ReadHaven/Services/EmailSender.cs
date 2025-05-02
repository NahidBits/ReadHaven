using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ReadHaven.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);

        Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            List<(byte[] Content, string FileName, string MimeType)> attachments
        );
    }

    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailSender(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:Server"];
            _smtpPort = int.Parse(configuration["EmailSettings:Port"]);
            _smtpUsername = configuration["EmailSettings:Email"];
            _smtpPassword = configuration["EmailSettings:Password"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || !IsValidEmail(toEmail))
                throw new ArgumentException("The 'toEmail' address is not valid.");

            if (string.IsNullOrWhiteSpace(_smtpUsername) || !IsValidEmail(_smtpUsername))
                throw new ArgumentException("The 'smtpUsername' address is not valid.");

            var mailMessage = new MailMessage(
                new MailAddress(_smtpUsername, "ReadHaven"),
                new MailAddress(toEmail)
            )
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            List<(byte[] Content, string FileName, string MimeType)> attachments)
        {
            if (string.IsNullOrWhiteSpace(toEmail) || !IsValidEmail(toEmail))
                throw new ArgumentException("The 'toEmail' address is not valid.");

            if (string.IsNullOrWhiteSpace(_smtpUsername) || !IsValidEmail(_smtpUsername))
                throw new ArgumentException("The 'smtpUsername' address is not valid.");

            var mailMessage = new MailMessage(
                new MailAddress(_smtpUsername, "ReadHaven"),
                new MailAddress(toEmail))
            {
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            foreach (var (content, fileName, mimeType) in attachments)
            {
                var stream = new MemoryStream(content);
                var attachment = new Attachment(stream, fileName, mimeType);
                mailMessage.Attachments.Add(attachment);
            }

            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
