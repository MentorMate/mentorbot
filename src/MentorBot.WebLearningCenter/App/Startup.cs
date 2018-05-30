// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MentorBot.WebLearningCenter
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
            services
                .AddMvc();

            services.AddSpaStaticFiles(configuration
                => configuration.RootPath = "ClientApp/dist");

            return services.BuildServiceProvider();
        }

        /// <summary>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</summary>
        public void Configure(IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var developmentEnvironment = app
                .ApplicationServices
                .GetService<IHostingEnvironment>()
                .IsDevelopment();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (developmentEnvironment)
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
