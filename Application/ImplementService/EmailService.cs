using Application.Handle.HandleEmail;
using Application.InterfaceService;
using Application.Payloads.Response;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ImplementService
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _configuration;
        public EmailService(EmailConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string SendEmail(EmailMessage emailMessage)
            {
            var message = CreateEmailMessage(emailMessage);
            Send(message);
            var recipients = string.Join(", ", message.To);
            return ResponseMessage.GetEmailSuccessMessage(recipients);
        }

        private MimeMessage CreateEmailMessage(EmailMessage message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _configuration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
            return emailMessage;
        }

        private void Send(MimeMessage message)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_configuration.SmtpServer, _configuration.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate("dongtimo0526@gmail.com", "ssas cvhe ofsz ydof");
                client.Send(message);
            }
            catch
            {
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
    }
}
