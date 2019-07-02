using System.Collections.Generic;

namespace MentorBot.Functions.Models.Settings
{
    /// <summary>Contains all the settings about a Command Processor.</summary>
    public class ProcessorSettings
    {
        /// <summary>Gets or sets the name of the command processor.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets a value indicating whether the command processor is enabled.</summary>
        public bool Enabled { get; set; }

        /// <summary>Gets or sets the setting configuration data.</summary>
        public IReadOnlyList<KeyValuePair<string, string>> Data { get; set; }
    }
}
