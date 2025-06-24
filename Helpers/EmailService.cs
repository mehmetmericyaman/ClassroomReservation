using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Ceng382ClassroomReservation.Helpers
{
    public static class EmailService
    {
        public static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = "mehmerc1905@gmail.com";
            var fromPassword = "ndyi nged annz qokt"; // Gmail App password

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            var message = new MailMessage(fromEmail, toEmail, subject, body);

            await smtpClient.SendMailAsync(message);
        }
    }
}
