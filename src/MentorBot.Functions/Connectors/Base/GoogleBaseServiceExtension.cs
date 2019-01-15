// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

using Google.Apis.Requests;

namespace MentorBot.Functions.Connectors.Base
{
    /// <summary>Some extensions that are used on google request.</summary>
    public static class GoogleBaseServiceExtension
    {
        /// <summary>Setups the specified request.</summary>
        /// <typeparam name="T">The actual request type.</typeparam>
        public static T Setup<T>(this T request, Action<T> setupAction)
            where T : IClientServiceRequest
        {
            setupAction(request);
            return request;
        }
    }
}
