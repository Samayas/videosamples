using Asp.Versioning;
using OpenAPIV3.Infrastructure;
using OpenAPIV3.Services;
using OpenAPIV3.Services.Interfaces;
using Scalar.AspNetCore;

namespace OpenAPIV3.Controllers
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
                options.AddDocumentTransformer(new ApiInfoDocumentTransformer("Weather API", "2.0", "Weather", "Weather Full Service"));
            });

            builder.Services.AddOpenApi("weathertemperature", options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
                options.ShouldInclude = (description) => description.GroupName == "weathertemperature";
                options.AddDocumentTransformer(new ApiInfoDocumentTransformer("Weather Temperature API", "1.1", "WeatherTemperature", "Weather Temperature Service"));
            });

            builder.Services.AddSingleton<IWeatherService, WeatherService>();

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();                  // /openapi/v1.json
                app.MapScalarApiReference(scalarOptions =>
                {
                    scalarOptions.Title = "Samayas Weather APIs";
                    scalarOptions.Servers = [ new ScalarServer("https://api.samayas.eu", "Prod"), new ScalarServer("https://localhost:7191", "Dev")];
                    scalarOptions.Theme = ScalarTheme.DeepSpace;
                    scalarOptions.Layout = ScalarLayout.Modern;
                    scalarOptions.DarkMode = false;
                    scalarOptions.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(
                        ScalarTarget.CSharp, ScalarClient.HttpClient
                    );
                    scalarOptions.EnabledTargets = [ScalarTarget.CSharp];
                    scalarOptions.DefaultOpenAllTags = true;
                    scalarOptions.Favicon = "/scalar/favicon.ico";
                    scalarOptions.AddDocuments(new[]
                    {
                        new ScalarDocument("weather","Weather API v2","/openapi/weather.json"),
                        new ScalarDocument("weathertemperature", "Weather Temperature API v1.1", "/openapi/weathertemperature.json")
                    });
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/scalar/favicon.ico", async (HttpContext context) =>
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = "OpenAPIV3.Assets.favicon.ico";
                Stream? stream = assembly.GetManifestResourceStream(resourceName);

                if (stream is null)
                    return Results.NotFound();

                context.Response.ContentType = "image/x-icon";
                await stream.CopyToAsync(context.Response.Body);
                return Results.Empty;
            });

            app.Run();
        }
    }
}