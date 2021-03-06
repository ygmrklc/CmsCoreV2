﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CmsCoreV2.Data;
using CmsCoreV2.Models;
using CmsCoreV2.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing.Constraints;
using Sakura.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using SaasKit.Multitenancy;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CmsCoreV2
{
    public class Startup
    {


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<HostDbContext>(options =>
                 options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMultitenancy<AppTenant, CachingAppTenantResolver>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddEntityFrameworkSqlServer().AddDbContext<ApplicationDbContext>();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddIdentity<ApplicationUser, Role>()
               .AddEntityFrameworkStores<ApplicationDbContext, Guid>()
               .AddDefaultTokenProviders();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddOptions();
            services.AddLocalization();
            services.AddBootstrapPagerGenerator(options =>
            {
                // Use default pager options.
                options.ConfigureDefault();
            });
            services.Configure<PagerOptions>(Configuration.GetSection("Pager"));
            services.AddMvc()
                .AddViewLocalization(
                    Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.SubFolder,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(
                    opts =>
                    {
                        var supportedCultures = new List<CultureInfo>
                        {
                new CultureInfo("en"),
                new CultureInfo("tr"),
                        };

                        opts.DefaultRequestCulture = new RequestCulture("tr");
            // Formatting numbers, dates, etc.
            opts.SupportedCultures = supportedCultures;
            // UI strings that we have localized.
            opts.SupportedUICultures = supportedCultures;
                    });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new TenantViewLocationExpander());
            });


            // Add application services.
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<CustomLocalizer, CustomLocalizer>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<IFeedbackService, FeedbackService>();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.CookieHttpOnly = true;
            });
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            


            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);
            // ana veritabanın kayıtları girilir
            app.ApplicationServices.GetRequiredService<HostDbContext>().Seed();
            // multi-tenancy kullanılmaya başlanılır
            app.UseMultitenancy<AppTenant>();
            app.UsePerTenantStaticFiles<AppTenant>("tenant", x => x.Folder);
            app.UseStaticFiles();
            // üyelik sistemi devreye alınır
            app.UseIdentity();



            app.UseSession();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "redirectRoute",
                   template: "{*oldUrl}",
                    defaults: new { controller = "Home", action = "RedirectToNewUrl" },
                    constraints: new
                    {
                        oldUrl = new RedirectRouteConstraint()
                    });
                routes.MapRoute(name: "formRoute",
                   template: "{culture}/form/{*slug}",
                    defaults: new { controller = "Home", action = "Form", culture = "no", slug = "" });
                routes.MapRoute(
                    name: "cultureRoute",
                    template: "{culture}/{*slug}",
                    defaults: new { controller = "Home", action = "Index", culture="no", slug = "anasayfa" },
                    constraints: new
                    {
                        culture = new RegexRouteConstraint("^[a-z]{2}(?:-[A-Z]{2})?$")
                    });

                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller=Dashboard}/{action=Index}");
                

                routes.MapRoute(name: "default",
                   template: "{controller=Home}/{action=Page404}");
            });



            
        }
    }
}