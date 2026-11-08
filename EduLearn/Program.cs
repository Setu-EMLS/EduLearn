using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduLearn.Infrastructure;
using EduLearn.Localization;
using EduLearn.Models;
using EduLearn.Services.Implementations;
using EduLearn.Services.Interfaces;

namespace EduLearn
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<ITranslator, Translator>();
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add<TranslateHtmlFilter>(); // translates the pages when the visitor chose Bangla
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHttpContextAccessor(); // ✅ IHttpContextAccessor registration

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ILearningService, LearningService>();
            builder.Services.AddScoped<IFileService, FileService>();

            builder.Services.AddAuthentication("MyCookieAuth")
                .AddCookie("MyCookieAuth", options =>
                {
                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Home/Logout";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            var app = builder.Build();

            await DbSeeder.SeedAsync(app.Services, app.Configuration, app.Logger);

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); 
            app.UseRouting();

            // English or Bangla, remembered in a cookie by /Language/Set (dates and numbers follow the language)
            var languages = new[] { new CultureInfo("en-US"), new CultureInfo("bn-BD") };
            var localization = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = languages,
                SupportedUICultures = languages
            };
            localization.RequestCultureProviders.Clear();
            localization.RequestCultureProviders.Add(new CookieRequestCultureProvider { CookieName = Translator.CookieName });
            app.UseRequestLocalization(localization);

            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapStaticAssets(); 
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            ).WithStaticAssets(); 

            app.Run();
        }
    }
}
