using BlogSlugify6DateSlugify.Processors;
using BlogSlugify6DateSlugify.Processors.Interfaces;

namespace BlogSlugify6DateSlugify
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

            // Configure the HTTP request pipeline.
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
            app.MapControllerRoute("blogArticle", "blog/{year:int}/{month:int}/{*article}", defaults: new { controller = "Blog", action = "Article" });
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
