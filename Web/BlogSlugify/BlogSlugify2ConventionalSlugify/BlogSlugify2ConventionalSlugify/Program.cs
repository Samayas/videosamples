using BlogSlugify2ConventionalSlugify.Processors;
using BlogSlugify2ConventionalSlugify.Processors.Interfaces;

namespace BlogSlugify2ConventionalSlugify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<IBlogProcessor, BlogProcessor>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/Error");

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute("blogIndex", "blog", defaults: new { controller = "Blog", action = "Index" });
            app.MapControllerRoute("blogSiteIndex", "blog/siteindex", defaults: new { controller = "Blog", action = "SiteIndex" });
            app.MapControllerRoute("blogArticle", "blog/{*article}", defaults: new { controller = "Blog", action = "Article" });
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
           
            app.Run();
        }
    }
}
// https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-9.0