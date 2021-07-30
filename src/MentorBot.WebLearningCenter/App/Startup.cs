using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MentorBot.WebLearningCenter
{
    /// <summary>The application startup class.</summary>
    public class Startup
    {
        private const string AngularStartScriptName = "start";
        private const string ClientAppDirectory = "ClientApp";

        private readonly IWebHostEnvironment _environment;

        /// <summary>Initializes a new instance of the <see cref="Startup"/> class.</summary>
        public Startup(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>This method gets called by the runtime. Use this method to add services to the container.</summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSpaStaticFiles(configuration =>
                configuration.RootPath = Path.Combine(ClientAppDirectory, "dist"));
        }

        /// <summary>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</summary>
        public void Configure(IApplicationBuilder app)
        {
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = ClientAppDirectory;

                if (_environment.IsDevelopment())
                {
                    spa.UseAngularCliServer(AngularStartScriptName);
                }
            });
        }
    }
}
