using PDUManagement.PDUBaseServiceAgent.Interfaces;
using PDUManagement.PDUBaseServiceAgent;

namespace PDUManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add Login hardcoded
            builder.Services.AddScoped<PDUDetails>(provider => new PDUDetails() { PDUServerAddress = "luxpdu.samayas.eu", UserName = "admin", Password = "admin" });

            // Add LogiLink Service Agent
            builder.Services.AddScoped<IPDUServiceAgent, LogiLinkServiceAgent.LogiLinkServiceAgent>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
