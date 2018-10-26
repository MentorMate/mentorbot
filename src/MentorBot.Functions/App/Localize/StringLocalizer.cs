// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Localize
{
    /// <summary>The default string localizer.</summary>
    /// <seealso cref="IStringLocalizer" />
    public sealed class StringLocalizer : IStringLocalizer
    {
        /// <inheritdoc/>
        public string this[string name, params object[] arguments] => name;
    }
}
