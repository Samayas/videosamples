using BlogSlugify3ConventionalExtraSlugify.Processors;
using BlogSlugify3ConventionalExtraSlugify.Processors.Interfaces;
using BlogSlugify3ConventionalExtraSlugify.Routing;

namespace BlogSlugify3ConventionalExtraSlugify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(mvcOptions =>
            {
                mvcOptions.Conventions.Add(new SlugifyControllerActionConvention());
            });

            builder.Services.AddRouting(routeOptions =>
            {
                routeOptions.ConstraintMap["slug"] = typeof(SlugConstraint);
                routeOptions.LowercaseUrls = true;
            });

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
            app.MapControllerRoute("blogArticle", "blog/{*article:slug}", defaults: new { controller = "Blog", action = "Article" });
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
           
            app.Run();
        }
    }
}
