using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace NOTNETPROJECT.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task SendVerificationEmailAsync(string toName, string toEmail, string verificationUrl)
        {
            var mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.From = new MailAddress(_configuration["Smtp:ToEmail"], "Your Website");
            mail.Subject = "Verify your email for Contact Form";
            mail.Body = $"Hello {toName},\n\nPlease verify your email by clicking the following link:\n{verificationUrl}\n\nIf you did not request this, ignore this email.";
            using (var smtp = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"])))
            {
                smtp.Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }

        public async Task SendEmailAsync(string fromName, string fromEmail, string message)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]);
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];
            var toEmail = _configuration["Smtp:ToEmail"];

            var mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.From = new MailAddress(fromEmail, fromName);
            mail.Subject = $"Contact form message from {fromName}";
            mail.Body = message;

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }
    }
}