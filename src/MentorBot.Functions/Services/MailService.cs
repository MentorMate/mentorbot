// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.SmtpClient;
using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Services
{
    /// <summary>The default e-mail service.</summary>
    public sealed class MailService : IMailService, IDisposable
    {
        /// <summary>The default timeout.</summary>
        public const int DefaultTimeout = 10000;

        private readonly ISmtpClient _client;
        private readonly SmtpOptions _options;

        /// <summary>Initializes a new instance of the <see cref="MailService"/> class.</summary>
        public MailService(ISmtpClient client, SmtpOptions options)
        {
            _options = options;
            _client = client;
        }

        /// <inheritdoc/>
        public bool IsActive =>
            !string.IsNullOrEmpty(_options?.SmtpHost) &&
            !string.IsNullOrEmpty(_options.MailFrom);

        /// <summary>Setups the SMTP client.</summary>
        public static void SetupSmtpClient(ISmtpClient client, SmtpOptions options)
        {
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.EnableSsl = options.SmtpUseSsl;
            client.Timeout = DefaultTimeout;
            client.Credentials = new NetworkCredential(options.SmtpUser, options.SmtpPassword);
        }

        /// <inheritdoc/>
        public async Task SendMailAsync(string subject, string message, params string[] emails)
        {
            if (!IsActive ||
                string.IsNullOrEmpty(subject) ||
                string.IsNullOrEmpty(message) ||
                emails.Length <= 0)
            {
                return;
            }

            var mail = new MailMessage
            {
                From = new MailAddress(_options.MailFrom, _options.MailFromName),
                Subject = subject,
                Body = message,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
            };

            foreach (var email in emails)
            {
                mail.To.Add(email);
            }

            SetupSmtpClient(_client, _options);
            await _client.SendMailAsync(mail);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}
