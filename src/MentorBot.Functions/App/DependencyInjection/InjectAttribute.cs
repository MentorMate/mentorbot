// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

using Microsoft.Azure.WebJobs.Description;

namespace MentorBot.Functions.App.DependencyInjection
{
    /// <summary>Inject a service.</summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
