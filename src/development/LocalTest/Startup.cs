using System;
using Altinn.Platform.Storage.Repository;
using LocalTest.Services.Storage.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;
using AltinnCore.Authentication.JwtCookie;
using AltinnCore.Authentication.Constants;
using Altinn.Common.PEP.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using LocalTest.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Http;
using LocalTest.Services.Profile.Interface;
using LocalTest.Services.Profile.Implementation;
using LocalTest.Services.Register.Interface;
using LocalTest.Services.Register.Implementation;
using LocalTest.Services.Authorization.Implementation;
using Altinn.Platform.Authorization.ModelBinding;
using Altinn.Authorization.ABAC.Interface;
using Altinn.Platform.Authorization.Services.Implementation;
using Altinn.Platform.Authorization.Repositories.Interface;
using Altinn.Platform.Authorization.Repositories;
using Altinn.Platform.Authorization.Services.Interface;
using Altinn.Platform.Storage.Helpers;
using Altinn.Common.PEP.Interfaces;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Clients;
using Microsoft.AspNetCore.Authorization;

namespace LocalTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Altinn.Common.PEP.Configuration.PepSettings>(Configuration.GetSection("PepSettings"));
            services.Configure<Altinn.Common.PEP.Configuration.PlatformSettings>(Configuration.GetSection("PlatformSettings"));

            services.Configure<LocalPlatformSettings>(Configuration.GetSection("LocalPlatformSettings"));
            services.AddControllersWithViews();
            services.AddSingleton(Configuration);
            services.Configure<GeneralSettings>(Configuration.GetSection("GeneralSettings"));
            services.Configure<Altinn.Platform.Authentication.Configuration.GeneralSettings>(Configuration.GetSection("AuthnGeneralSettings"));
            services.Configure<CertificateSettings>(Configuration);
            services.Configure<CertificateSettings>(Configuration.GetSection("CertificateSettings"));
            services.AddSingleton<IUserProfiles, UserProfilesWrapper>();
            services.AddSingleton<IOrganizations, OrganizationsWrapper>();
            services.AddSingleton<Services.Register.Interface.IParties, PartiesWrapper>();
            services.AddSingleton<IPersons, PersonsWrapper>();
            services.AddSingleton<Altinn.Platform.Authorization.Services.Interface.IParties, PartiesService>();
            services.AddSingleton<IInstanceRepository, InstanceRepository>();
            services.AddSingleton<IDataRepository, DataRepository>();
            services.AddSingleton<IInstanceEventRepository, InstanceEventRepository>();
            services.AddSingleton<IApplicationRepository, ApplicationRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IPDP, PDPAppSI>();

            services.AddTransient<IAuthorizationHandler, AppAccessHandler>();
            services.AddTransient<IAuthorizationHandler, ScopeAccessHandler>();

            services.AddSingleton<IContextHandler, ContextHandler>();
            services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPoint>();
            services.AddSingleton<IPolicyInformationRepository, PolicyInformationRepository>();
            services.AddSingleton<IRoles, RolesWrapper>();

            X509Certificate2 cert = new X509Certificate2("JWTValidationCert.cer");
            SecurityKey key = new X509SecurityKey(cert);

            services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
                .AddJwtCookie(options =>
                {
                    var generalSettings = Configuration.GetSection("GeneralSettings").Get<GeneralSettings>();
                    options.ExpireTimeSpan = new TimeSpan(0, 30, 0);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Cookie.Domain = "altinn3local.no";
                    options.Cookie.Name = "AltinnStudioRuntime";
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthzConstants.POLICY_INSTANCE_READ, policy => policy.Requirements.Add(new AppAccessRequirement("read")));
                options.AddPolicy(AuthzConstants.POLICY_INSTANCE_WRITE, policy => policy.Requirements.Add(new AppAccessRequirement("write")));
                options.AddPolicy(AuthzConstants.POLICY_SCOPE_APPDEPLOY, policy => policy.Requirements.Add(new ScopeAccessRequirement("altinn:appdeploy")));
                options.AddPolicy(AuthzConstants.POLICY_SCOPE_INSTANCE_READ, policy => policy.Requirements.Add(new ScopeAccessRequirement("altinn:instances.read")));
            });


            services.AddMvc(options =>
            {
                // Adding custom modelbinders
                options.ModelBinderProviders.Insert(0, new XacmlRequestApiModelBinderProvider());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable higher level of detail in exceptions related to JWT validation
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
