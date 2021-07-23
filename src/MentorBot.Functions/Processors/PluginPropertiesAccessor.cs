using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;

namespace MentorBot.Functions.Processors
{
    /// <summary>The default property accessor implementation.</summary>
    public sealed class PluginPropertiesAccessor : IPluginPropertiesAccessor
    {
        private readonly string _email;
        private readonly Plugin _plugin;
        private readonly IStorageService _storageService;

        private User _user;

        /// <summary>Initializes a new instance of the <see cref="PluginPropertiesAccessor"/> class.</summary>
        private PluginPropertiesAccessor(
            string email,
            Plugin plugin,
            IStorageService storageService)
        {
            _email = email;
            _plugin = plugin;
            _storageService = storageService;
        }

        /// <summary>Gets a <see cref="PluginPropertiesAccessor"/> class instance.</summary>
        public static PluginPropertiesAccessor GetInstance(string email, Plugin plugin, IStorageService storageService) =>
            new PluginPropertiesAccessor(email, plugin, storageService);

        /// <inheritdoc />
        public IReadOnlyList<T> GetAllPluginPropertyValues<T>(string uniqueName) =>
            _plugin.Groups?.Select(it => it.Values).GetGroupsValues<T>(uniqueName) ?? new T[0];

        /// <inheritdoc />
        public IReadOnlyList<IReadOnlyList<PluginPropertyValue>> GetPluginPropertyGroup(string groupName) =>
            _plugin.Groups?.FirstOrDefault(group => groupName.Equals(group.UniqueName, StringComparison.InvariantCulture))?.Values
            ?? Array.Empty<PluginPropertyValue[]>();

        /// <inheritdoc />
        public async Task<IReadOnlyList<T>> GetAllUserPropertyValuesAsync<T>(string uniqueName)
        {
            if (_user == null)
            {
                if (string.IsNullOrEmpty(_email))
                {
                    return new T[0];
                }

                _user = await _storageService.GetUserByEmailAsync(_email);
            }

            return _user.Properties.GetAllUserValues<T>(uniqueName);
        }
    }
}
