using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;
using Swashbuckle.AspNetCore.Swagger;
using de.uni_paderborn.si_lab.hip.featuretoggles.data;

namespace HiP_FeatureToggles
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
			// Adding Cross Orign Requests 
			services.AddCors();

			// Add database service for Postgres
			services.AddDbContext<FeatureToggleDbContext>(options => options.UseNpgsql(appConfig.DatabaseConfig.ConnectionString));

            // Add framework services.
            services.AddMvc();

			// Add Swagger service
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info() {
					Title = "HiP-Feature-Toggle API",
					Version = "v1"
				});

				c.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Api.xml"));
				c.OperationFilter<SwaggerOperationFilter>();
			});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
