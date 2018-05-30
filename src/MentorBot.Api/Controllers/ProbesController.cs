// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Reflection;

using Microsoft.AspNetCore.Mvc;

namespace MentorBot.Api.Controllers
{
    /// <summary>And enpoint for testing api availability.</summary>
    [Route("api/[controller]")]
    public class ProbesController : Controller
    {
        /// <summary>Return the api version string.</summary>
        [HttpGet]
        public IActionResult Get() =>
            Ok(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
    }
}
