using System;
using System.Collections.Generic;
using System.Linq;

using MentorBot.Functions.Models.Domains.Plugins;

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>A plugin property value extensions.</summary>
    public static class PluginPropertyValueExtensions
    {
        /// <summary>Gets the value of a plugin property.</summary>
        /// <typeparam name="T">The type of property value.</typeparam>
        public static T GetValue<T>(this IEnumerable<PluginPropertyValue> values, string uniqueName) =>
            values.GetValues<T>(uniqueName).FirstOrDefault();

        /// <summary>Gets all plugin property values filtered by unique name.</summary>
        /// <typeparam name="T">The type of property value.</typeparam>
        public static IReadOnlyList<T> GetValues<T>(this IEnumerable<PluginPropertyValue> values, string uniqueName) =>
            values.Where(it => uniqueName.Equals(it.Key, StringComparison.InvariantCulture)).Select(it => CastValue<T>(it.Value)).ToArray();

        /// <summary>Gets all user values.</summary>
        /// <typeparam name="T">The type of property value.</typeparam>
        public static IReadOnlyList<T> GetAllUserValues<T>(this IDictionary<string, PluginPropertyValue[][]> properties, string uniqueName) =>
            properties?.Values.GetGroupsValues<T>(uniqueName) ?? new T[0];

        /// <summary>Gets plugin groups values.</summary>
        /// <typeparam name="T">The type of property value.</typeparam>
        public static IReadOnlyList<T> GetGroupsValues<T>(this IEnumerable<PluginPropertyValue[][]> groups, string uniqueName) =>
            groups?
                .SelectMany(group => group?.Where(values => values != null).SelectMany(values => values) ?? new PluginPropertyValue[0])
                .GetValues<T>(uniqueName) ?? new T[0];

        private static T CastValue<T>(object value)
        {
            var typeOfT = typeof(T);
            if (value is string stringValue)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    return default;
                }

                if (typeOfT == typeof(bool))
                {
                    return (T)(object)(stringValue.ToLowerInvariant() == "true");
                }
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
