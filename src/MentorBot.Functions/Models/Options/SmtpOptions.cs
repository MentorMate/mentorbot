// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Globalization;

using Microsoft.Extensions.Configuration;

namespace MentorBot.Functions.Models.Options
{
    /// <summary>The SMTP mail send options.</summary>
    public sealed class SmtpOptions
    {
        /// <summary>Initializes a new instance of the <see cref="SmtpOptions"/> class.</summary>
        public SmtpOptions(IConfiguration configuration)
            : this(
                  configuration[nameof(SmtpHost)],
                  int.TryParse(configuration[nameof(SmtpPort)], NumberStyles.Integer, CultureInfo.InvariantCulture, out int port) ? port : default,
                  configuration[nameof(MailFrom)],
                  configuration[nameof(MailFromName)],
                  configuration[nameof(SmtpUser)],
                  configuration[nameof(SmtpPassword)],
                  !bool.TryParse(configuration[nameof(SmtpUseSsl)], out bool ssl) || ssl)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SmtpOptions"/> class.</summary>
        public SmtpOptions(string smtpHost, int smtpPort, string mailFrom, string mailFromName, string smtpUser, string smtpPassword, bool smtpUseSsl)
        {
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            MailFrom = mailFrom;
            MailFromName = mailFromName;
            SmtpUser = smtpUser;
            SmtpPassword = smtpPassword;
            SmtpUseSsl = smtpUseSsl;
        }

        /// <summary>Gets or sets the SMTP host.</summary>
        public string SmtpHost { get; set; }

        /// <summary>Gets or sets the SMTP port.</summary>
        public int SmtpPort { get; set; }

        /// <summary>Gets or sets the mail from.</summary>
        public string MailFrom { get; set; }

        /// <summary>Gets or sets the name of the mail from.</summary>
        public string MailFromName { get; set; }

        /// <summary>Gets or sets the SMTP user.</summary>
        public string SmtpUser { get; set; }

        /// <summary>Gets or sets the SMTP password.</summary>
        public string SmtpPassword { get; set; }

        /// <summary>Gets or sets a value indicating whether [SMTP use SSL].</summary>
        public bool SmtpUseSsl { get; set; }
    }
}
