using System.Net.Mail;
using System.Net;

namespace WebBanNuocMVC.DesignPatterns.Adapter
{
    public class EmailNotificationAdapter : INotificationAdapter
    {
        public async Task SendAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("yenphuongtyp24@gmail.com", "ecej zmbm vzhm yzwz"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("yenphuongtyp24@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
