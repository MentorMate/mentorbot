using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace MentorBot.Tests._Base
{
    public static class MockFunction
    {
        public static FunctionContext GetContext(params ServiceDescriptor[] descriptors)
        {
            var context = Substitute.For<FunctionContext>();
            if (descriptors != null && descriptors.Any())
            {
                var logger = Substitute.For<ILogger>();
                var loggerFactory = Substitute.For<ILoggerFactory>();
                var services = descriptors.Concat(
                    new[]
                    {
                        new ServiceDescriptor(typeof(ILoggerFactory), loggerFactory)
                    })
                    .ToArray();

                loggerFactory.CreateLogger(string.Empty).ReturnsForAnyArgs(logger);
                context.InstanceServices.Returns(new ServiceProvider(services));
                context.GetLogger(string.Empty).ReturnsForAnyArgs(logger);
            }

            return context;
        }

        public static HttpRequestData GetRequest(string requestContent, FunctionContext context)
        {
            var data = Substitute.For<HttpRequestData>(context);
            var res = new MockHttpResponseData(context);
            data.CreateResponse().Returns(res);

            if (!string.IsNullOrEmpty(requestContent))
            {
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(requestContent));
                data.Body.Returns(stream);
            }

            return data;
        }

        internal class ServiceProvider : IServiceProvider
        {
            public readonly IReadOnlyDictionary<Type, object> _services;

            public ServiceProvider(ServiceDescriptor[] descriptors)
            {
                _services = descriptors.ToDictionary(it => it.ServiceType, it => it.ImplementationInstance);
            }

            /// <inheritdoc/>
            public object GetService(Type serviceType) =>
                _services.GetValueOrDefault(serviceType);
        }
    }
}
