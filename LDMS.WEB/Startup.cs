using LDMS.Core;
using LDMS.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;

namespace LDMS.WEB
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
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(120);
            });
            services.AddAuthorization();

            services.AddMvc(config =>
            {
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            AppSettings appSettings = new AppSettings();
            Configuration.GetSection("AppSettings").Bind(appSettings);

            LdapSettings ldapSettings = new LdapSettings();
            Configuration.GetSection("LdapSettings").Bind(ldapSettings);

            JwtSettings jwtSettings = new JwtSettings();
            Configuration.GetSection("JwtSettings").Bind(jwtSettings);

            services.AddSingleton(appSettings);
            services.AddSingleton(ldapSettings);
            services.AddSingleton(jwtSettings);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache();
            services.AddControllers();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.ConfigureJWTService(jwtSettings);
            services.ConfigureAppServices(Configuration);
            services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services
          .AddAuthentication()
          .AddCookie(options =>
          {
              options.LoginPath = "/Account/Index";
              options.LogoutPath = "/Account/logout";
              options.SlidingExpiration = true;
          });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthorization();
            app.UseAuthentication();
            app.ConfigureJWT();
            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
            };
            app.UseCookiePolicy(cookiePolicyOptions);
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(name: "Instructor", pattern: "{controller=Instructor}/{action=Add}/{id?}");
                endpoints.MapControllerRoute(name: "Account", pattern: "{controller=Account}/{action=Index}");
            });

            app.UseStatusCodePages(async context =>
            { 
                if (context.HttpContext.Response.StatusCode == (int)System.Net.HttpStatusCode.Unauthorized ||
                    context.HttpContext.Response.StatusCode == (int)System.Net.HttpStatusCode.Forbidden)
                    context.HttpContext.Response.Redirect("/Account/Index");
            });
        }
    }
}
