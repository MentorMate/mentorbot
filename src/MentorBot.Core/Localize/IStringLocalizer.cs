// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Core.Localize
{
    /// <summary>A string localizer.</summary>
    public interface IStringLocalizer
    {
        /// <summary>Gets the localized string with the specified name.</summary>
        string this[string name, params object[] arguments] { get; }
    }
}
