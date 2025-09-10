using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Syncfusion.Licensing;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
using VillaHub.Infrastructure.Data;
using VillaHub.Infrastructure.Repository;
using VillaHub.Web.SignalR;

namespace VillaHub.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // -----------------------------
            // Culture Configuration
            // -----------------------------
            var cultureInfo = new CultureInfo("en-US")
            {
                DateTimeFormat = { Calendar = new GregorianCalendar() }
            };
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ar-AE")
                };

                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // -----------------------------
            // MVC + Localization
            // -----------------------------
            builder.Services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            // -----------------------------
            // Database & Identity
            // -----------------------------
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("PublishConnection"),
                
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,              // Number of retries
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
                    errorNumbersToAdd: null       // Additional SQL error numbers to retry
                )
             ));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(option =>
            {
                option.Password.RequiredLength = 6;
                option.SignIn.RequireConfirmedEmail = true;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                // options.AccessDeniedPath = "/Home/Account/AccessDenied";
            });

            // -----------------------------
            // Dependency Injection
            // -----------------------------
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<ICustomEmailSender, EmailSender>();
            builder.Services.AddTransient<TwilioService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // -----------------------------
            // External Authentication
            // -----------------------------
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                })
                .AddFacebook(options =>
                {
                    options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
                    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
                });

            // -----------------------------
            // Third-Party Integrations
            // -----------------------------
            SyncfusionLicenseProvider.RegisterLicense(
                builder.Configuration.GetSection("Syncfusion:Licensekey").Get<string>());

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Strip:SecretKey").Get<string>();

            builder.Services.AddSignalR();

            // -----------------------------
            // Build App
            // -----------------------------
            var app = builder.Build();

            // -----------------------------
            // Middleware Pipeline
            // -----------------------------
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseAuthentication();
            app.UseAuthorization();

            // -----------------------------
            // Endpoints
            // -----------------------------
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<BookingHub>("/bookingHub");

            // Apply migrations at startup
            //using (var scope = app.Services.CreateScope())
            //{
            //    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //    db.Database.Migrate(); // applies any pending migrations
            //}

            app.Run();
        }
    }

   
}
