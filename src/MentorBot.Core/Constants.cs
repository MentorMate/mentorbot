﻿// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Diagnostics.CodeAnalysis;

namespace MentorBot.Core
{
    /// <summary>Contains all global application constant.</summary>
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        /// <summary>Gets the swagger json endpoint.</summary>
        public static string SwaggerJsonEndpoint => "/swagger/v1/swagger.json";
    }
}
