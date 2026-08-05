using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SmtpEmailSender
{
    public class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
    }

    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IOptions<SmtpOptions> _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options;
        }

        public string? LastSentTo { get; set; }
        public SmtpOptions Options => _options.Value;

        public async Task SendAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient(Options.Host);
            if (Options.Port != 0 && Options.Port != 587) // SMTP
                await smtpClient.ConnectAsync(Options.Port, Options.Username ?? "");
            else // IMAP/POP3
                await smtpClient.ConnectAsync(Options.Port);

            var message = new MailMessage();
            message.From = new MailAddress(Options.Username ?? "", "Sender");
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;

            using (var stream = new MemoryStream())
            {
                await message.WriteToAsync(stream, System.Net.Mail.MailVersion.Default);

                if (Options.Port != 0 && Options.Port != 587) // SMTP
                    await smtpClient.SendMailAsync(stream);
                else // IMAP/POP3
                    throw new NotImplementedException("Sending via IMAP/POP3 is not supported yet");
            }

            LastSentTo = to;
        }
    }
}