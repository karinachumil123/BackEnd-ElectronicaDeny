using BackEnd_ElectronicaDeny.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MimeKit;

namespace BackEnd_ElectronicaDeny.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<EmailConfiguration> emailConfiguration)
        {
            _smtpSettings = emailConfiguration.Value.SmtpSettings;
        }

        public async Task SendEmailAsync(EmailModel emailModel)
        {
            if (string.IsNullOrEmpty(emailModel.Email))
            {
                throw new ArgumentException("El correo electrónico no puede ser nulo o vacío", nameof(emailModel.Email));
            }

            if (string.IsNullOrEmpty(_smtpSettings.SenderEmail))
            {
                throw new ArgumentException("El correo electrónico del remitente no puede ser nulo o vacío", nameof(_smtpSettings.SenderEmail));
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(emailModel.Email));
            message.Subject = emailModel.Subject;
            message.Body = new TextPart("plain")
            {
                Text = emailModel.Message
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(_smtpSettings.Server, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
                await client.SendAsync(message);
                client.Disconnect(true);
            }

        }
    }
}