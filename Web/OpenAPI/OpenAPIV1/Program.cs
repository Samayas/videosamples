using OpenAPIV1.Services;
using OpenAPIV1.Services.Interfaces;
using Scalar.AspNetCore;

namespace OpenAPIV1.Controllers
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            builder.Services.AddOpenApi(options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
            });

            builder.Services.AddSingleton<IWeatherService, WeatherService>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();                  // /openapi/v1.json
                app.MapScalarApiReference();       // /scalar/v1
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}