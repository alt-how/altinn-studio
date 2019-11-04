using System;
using System.IO;
using System.Reflection;
using AltinnCore.Designer.Infrastructure;
using AltinnCore.Designer.Infrastructure.Authorization;
using AltinnCore.Designer.TypedHttpClients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace AltinnCore.Designer
{
    /// <summary>
    /// This is the class that set up the application during startup
    /// <see href="https://docs.asp.net/en/latest/fundamentals/startup.html#the-startup-class"/>
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Gets the application configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class
        /// </summary>
        /// <param name="configuration">the configuration for designer</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures the services available for the asp.net Core application
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup#the-configureservices-method"/>
        /// </summary>
        /// <param name="services">The services available for asp.net Core</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.RegisterServiceImplementations(Configuration);
            services.RegisterIntegrations(Configuration);

            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddResponseCompression();

            CreateDirectory();

            services.ConfigureMvc();
            services.ConfigureSettings(Configuration);
            services.RegisterTypedHttpClients(Configuration);
            services.ConfigureAuthentication();
            services.ConfigureApplicationInsight();
            services.ConfigureLocalization();
            services.AddPolicyBasedAuthorization();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Altinn Designer API", Version = "v1" });
                try
                {
                    c.IncludeXmlComments(GetXmlCommentsPathForControllers());
                }
                catch
                {
                    // Catch swashbuckle exception if it doesn't find the generated XML documentation file
                }
            });
        }

        /// <summary>
        /// Configure the application.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup#the-configure-method"/>
        /// </summary>
        /// <param name="appBuilder">The application builder</param>
        /// <param name="env">Hosting environment</param>
        public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                appBuilder.UseExceptionHandler("/error-local-development");
            }
            else
            {
                appBuilder.UseExceptionHandler("/error");
            }

            appBuilder.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    ResponseHeaders headers = context.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(60),
                    };
                },
            });

            const string swaggerRoutePrefix = "designer/swagger";
            appBuilder.UseSwagger(c =>
            {
                c.RouteTemplate = swaggerRoutePrefix + "/{documentName}/swagger.json";
            });
            appBuilder.UseSwaggerUI(c =>
            {
                c.RoutePrefix = swaggerRoutePrefix;
                c.SwaggerEndpoint($"/{swaggerRoutePrefix}/v1/swagger.json", "Altinn Designer API V1");
            });

            appBuilder.UseRouting();

            // appBuilder.UseHsts();
            // appBuilder.UseHttpsRedirection();
            appBuilder.UseAuthentication();
            appBuilder.UseAuthorization();

            appBuilder.UseResponseCompression();
            appBuilder.UseRequestLocalization();
            
            appBuilder.UseEndpoints(endpoints =>
            {
                // ------------------------- DEV ----------------------------- //
                endpoints.MapControllerRoute(
                    name: "orgRoute",
                    pattern: "designer/{org}/{controller}/{action=Index}/",
                    defaults: new { controller = "Config" },
                    constraints: new
                    {
                        controller = "Codelist|Config",
                    });

                endpoints.MapControllerRoute(
                        name: "serviceDevelopmentRoute",
                        pattern: "designer/{org}/{app}",
                        defaults: new { controller = "ServiceDevelopment", action = "index" });

                endpoints.MapControllerRoute(
                    name: "designerApiRoute",
                    pattern: "designerapi/{controller}/{action=Index}/{id?}",
                    defaults: new { controller = "Repository" },
                    constraints: new
                    {
                        controller = @"(Repository|Language|User)",
                    });
                endpoints.MapControllerRoute(
                          name: "serviceRoute",
                          pattern: "designer/{org}/{app}/{controller}/{action=Index}/{id?}",
                          defaults: new { controller = "Service" },
                          constraints: new
                          {
                              controller = @"(Codelist|Config|Service|RuntimeAPI|ManualTesting|Model|Rules|ServiceMetadata|Text|UI|UIEditor|ServiceDevelopment)",
                              app = "[a-zA-Z][a-zA-Z0-9_\\-]{2,30}",
                              id = "[a-zA-Z0-9_\\-]{1,30}",
                          });

                endpoints.MapControllerRoute(
                       name: "applicationMetadataApiRoute",
                       pattern: "designer/api/v1/{org}/{app}",
                       defaults: new { controller = "ApplicationMetadata", action = "ApplicationMetadata" });

                endpoints.MapControllerRoute(
                        name: "reposRoute",
                        pattern: "{controller}/{action}/",
                        defaults: new { controller = "RedirectController" });

                // -------------------------- DEFAULT ------------------------- //
                endpoints.MapControllerRoute(
                   name: "defaultRoute2",
                   pattern: "{controller}/{action=StartPage}/{id?}",
                   defaults: new { controller = "Home" });

                endpoints.MapControllerRoute(
                    name: "defaultRoute",
                    pattern: "{action=StartPage}/{id?}",
                    defaults: new { controller = "Home" });
            });
        }

        private void CreateDirectory()
        {
            // TODO: Figure out how appsettings.json parses values and merges with environment variables and use these here.
            // Since ":" is not valid in environment variables names in kubernetes, we can't use current docker-compose environment variables
            string repoLocation = (Environment.GetEnvironmentVariable("ServiceRepositorySettings__RepositoryLocation") != null)
                                ? Environment.GetEnvironmentVariable("ServiceRepositorySettings__RepositoryLocation")
                                : Configuration["ServiceRepositorySettings:RepositoryLocation"];

            if (!Directory.Exists(repoLocation))
            {
                Directory.CreateDirectory(repoLocation);
            }
        }

        private static string GetXmlCommentsPathForControllers()
        {
            // locate the xml file being generated by .NET
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XML";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            return xmlPath;
        }
    }
}
