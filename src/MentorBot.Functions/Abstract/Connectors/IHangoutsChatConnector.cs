// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Abstract.Connectors
{
    /// <summary>Google chat connector.</summary>
    public interface IHangoutsChatConnector : IAsyncResponder
    {
        /// <summary>Gets the private chat channels addresses.</summary>
        IReadOnlyList<GoogleChatAddress> GetPrivateAddress(IReadOnlyList<string> filterSpaces);

        /// <summary>Gets the rooms channels addresses.</summary>
        GoogleChatAddress GetAddressByName(string name);
    }
}
