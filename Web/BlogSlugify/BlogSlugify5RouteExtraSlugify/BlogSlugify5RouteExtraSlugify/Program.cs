using BlogSlugify5RouteExtraSlugify.Processors;
using BlogSlugify5RouteExtraSlugify.Processors.Interfaces;
using BlogSlugify5RouteExtraSlugify.Routing;
using BlogSlugify4RouteExtraSlugify.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BlogSlugify5RouteExtraSlugify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(mvcOptions =>
            {
                // Attribute Based
                mvcOptions.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            });

            builder.Services.AddRouting(routeOptions =>
            {
                routeOptions.ConstraintMap["slug"] = typeof(SlugConstraint);
                routeOptions.LowercaseUrls = true;
            });

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
            app.MapControllers();

         //   app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
