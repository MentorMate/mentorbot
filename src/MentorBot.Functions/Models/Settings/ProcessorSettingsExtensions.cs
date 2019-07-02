using System.Collections.Generic;
using System.Linq;

namespace MentorBot.Functions.Models.Settings
{
    /// <summary>A extensions for the ProcessorSettings.</summary>
    public static class ProcessorSettingsExtensions
    {
        /// <summary>Return data as dictionary.</summary>
        public static IReadOnlyDictionary<string, string> DataAsDictionary(this ProcessorSettings settings) =>
            settings == null || settings.Data == null ? null : new Dictionary<string, string>(settings.Data);

        /// <summary>Gets the data with key as array of strings.</summary>
        public static string[] GetAsArray(this IReadOnlyDictionary<string, string> data, string key) =>
            data.GetValueOrDefault(key)?.Split(';').Select(it => it.Trim(' ', '"', '\'')).ToArray();
    }
}
