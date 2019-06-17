using System.Collections.Generic;

using MentorBot.Functions.Processors;

namespace MentorBot.Functions.Models.Settings
{
    /// <summary>Hold the default settings.</summary>
    public static class Default
    {
        /// <summary>The default excluded client key.</summary>
        public const string DefaultExcludedClientKey = "ExcludedClients";

        /// <summary>The notify by email key.</summary>
        public const string NotifyByEmailKey = "NotifyByEmail";

        /// <summary>The email key.</summary>
        public const string EmailKey = "Email";

        /// <summary>Gets the default settings.</summary>
        public static MentorBotSettings DefaultSettings => new MentorBotSettings
        {
            Processors = new[]
            {
                new ProcessorSettings
                {
                    Name = nameof(CalendarProcessor),
                    Enabled = true
                },
                new ProcessorSettings
                {
                    Name = nameof(LocalTimeProcessor),
                    Enabled = true
                },
                new ProcessorSettings
                {
                    Name = nameof(OpenAirProcessor),
                    Enabled = true,
                    Data = new[]
                    {
                        new KeyValuePair<string, string>(DefaultExcludedClientKey, string.Empty),
                        new KeyValuePair<string, string>(NotifyByEmailKey, "false"),
                        new KeyValuePair<string, string>(EmailKey, string.Empty)
                    }
                },
                new ProcessorSettings
                {
                    Name = nameof(RepeatProcessor),
                    Enabled = true
                }
            }
        };
    }
}
