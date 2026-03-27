using Asp.Versioning;
using OpenAPIV2.Services;
using OpenAPIV2.Services.Interfaces;
using Scalar.AspNetCore;

namespace OpenAPIV2.Controllers
{
    public class Program
    {
        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            builder.Services.AddProblemDetails();

            builder.Services.AddOpenApi("weather", options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
                options.ShouldInclude = (description) => description.GroupName == "weather";
            });

            builder.Services.AddOpenApi("weathertemperature", options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
                options.ShouldInclude = (description) => description.GroupName == "weathertemperature";
            });

            builder.Services.AddSingleton<IWeatherService, WeatherService>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();                  // /openapi/v1.json
                app.MapScalarApiReference(scalarOptions =>
                {
                    scalarOptions.AddDocuments(new[]
                    {
                        new ScalarDocument("weather","Weather API v1","/openapi/weather.json"),
                        new ScalarDocument("weathertemperature", "Weather Temperature API v1", "/openapi/weathertemperature.json")
                    });
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}