// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

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
            values.Where(it => it.Key.Equals(uniqueName, StringComparison.InvariantCulture)).Select(it => (T)it.Value).ToArray();
    }
}
