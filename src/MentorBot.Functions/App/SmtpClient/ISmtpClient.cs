using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MentorBot.Functions.App.SmtpClient
{
    /// <summary>A smtp client interface.</summary>
    public interface ISmtpClient : IDisposable
    {
        /// <summary>Gets or sets the delivery method.</summary>
        SmtpDeliveryMethod DeliveryMethod { get; set; }

        /// <summary>Gets or sets the SMTP host.</summary>
        string Host { get; set; }

        /// <summary>Gets or sets the SMTP port.</summary>
        int Port { get; set; }

        /// <summary>Gets or sets a value indicating whether [use default credentials].</summary>
        bool UseDefaultCredentials { get; set; }

        /// <summary>Gets or sets a value indicating whether [the SmtpClient uses Secure Sockets Layer (SSL) to encrypt the connection].</summary>
        bool EnableSsl { get; set; }

        /// <summary>Gets or sets the amount of time after which a call timeouts.</summary>
        int Timeout { get; set; }

        /// <summary>Gets or sets the network credentials.</summary>
        ICredentialsByHost Credentials { get; set; }

        /// <summary>Sends the mail asynchronous.</summary>
        /// <param name="message">The mail message.</param>
        Task SendMailAsync(MailMessage message);
    }
}
