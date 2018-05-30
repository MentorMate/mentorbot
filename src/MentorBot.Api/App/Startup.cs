// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.IO;
using System.Reflection;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using MentorBot.Api.App.Filters;
using MentorBot.Core;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

using Swashbuckle.AspNetCore.Swagger;

namespace MentorBot.Api
{
    /// <summary>The application startup class.</summary>
    public class Startup : IStartup
    {
        private readonly IConfiguration _configuration;

        /// <summary>Initializes a new instance of the <see cref="Startup"/> class.</summary>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>This method gets called by the runtime. Use this method to add services to the container.</summary>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(RestGlobalExceptionFilter));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info());

                var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, typeof(Startup).Assembly.GetName().Name + ".xml");
                c.IncludeXmlComments(filePath);
            });

            var containerOptions = new ContainerOptions { EnablePropertyInjection = false };
            using (var container = new ServiceContainer(containerOptions))
            {
                // Register all services.
                container.RegisterAssembly(
                    Assembly.Load(new AssemblyName("MentorBot.Business")),
                    () => new PerScopeLifetime(),
                    (service, implementation) =>
                        service.GetTypeInfo().IsAbstract &&
                        service.Name != nameof(IDisposable) &&
                        service.Namespace.StartsWith("MentorBot.Core.Abstract", StringComparison.InvariantCulture));

                return container.CreateServiceProvider(services);
            }
        }

        /// <summary>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(Constants.SwaggerJsonEndpoint, string.Empty);
            });
        }
    }
}
