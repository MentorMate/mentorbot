// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Models.Domains.Plugins;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>A plugin propery values accessor.</summary>
    public interface IPluginPropertiesAccessor
    {
        /// <summary>Gets the global plugin property values.</summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        IReadOnlyList<T> GetAllPluginPropertyValues<T>(string uniqueName);

        /// <summary>Gets the plugin property group of values.</summary>
        IReadOnlyList<IReadOnlyList<PluginPropertyValue>> GetPluginPropertyGroup(string groupName);

        /// <summary>Gets the user related property values asyncronius.</summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        Task<IReadOnlyList<T>> GetAllUserPropertyValuesAsync<T>(string uniqueName);
    }
}
