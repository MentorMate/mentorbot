// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Diagnostics.CodeAnalysis;

namespace MentorBot.Functions
{
    /// <summary>Contains all global application constant.</summary>
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        /// <summary>Gets the settings cache key.</summary>
        public static string PluginsCacheKey => "plugins";
    }
}
