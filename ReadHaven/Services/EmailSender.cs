using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ReadHaven.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
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
            {
                throw new ArgumentException("The 'toEmail' address is not valid.");
            }

            if (string.IsNullOrWhiteSpace(_smtpUsername) || !IsValidEmail(_smtpUsername))
            {
                throw new ArgumentException("The 'smtpUsername' address is not valid.");
            }

            var fromEmail = new MailAddress(_smtpUsername, "ReadHaven");
            var toEmailAddress = new MailAddress(toEmail);

            var mailMessage = new MailMessage(fromEmail, toEmailAddress)
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);  // API key in place of password
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
            }
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
