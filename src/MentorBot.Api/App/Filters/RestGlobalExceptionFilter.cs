// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MentorBot.Api.App.Filters
{
    /// <summary>The global exception filter.</summary>
    /// <seealso cref="IExceptionFilter" />
    public class RestGlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="RestGlobalExceptionFilter" /> class.</summary>
        public RestGlobalExceptionFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RestGlobalExceptionFilter>();
        }

        /// <summary>
        /// Called after an action has thrown an <see cref="T:System.Exception" />.
        /// </summary>
        /// <param name="context">The <see cref="T:ExceptionContext" />.</param>
        public void OnException(ExceptionContext context)
        {
            if (context != null)
            {
                var response = new
                {
                    context.Exception.Message,
                    context.Exception.StackTrace
                };

                context.Result = new JsonResult(response)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };

                _logger.LogError(new EventId(0), context.Exception, context.Exception.Message);
            }
        }
    }
}
