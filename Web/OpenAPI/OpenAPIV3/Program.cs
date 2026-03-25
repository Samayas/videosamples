using Asp.Versioning;
using Microsoft.Extensions.Options;
using OpenAPIV3.Infrastructure;
using OpenAPIV3.Infrastructure;
using OpenAPIV3.Services;
using OpenAPIV3.Services.Interfaces;
using Scalar.AspNetCore;

namespace OpenAPIV1.Controllers
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
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;  // <-- this is the fix
            });
            builder.Services.AddProblemDetails();
          
            builder.Services.AddOpenApi("weather", options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
                options.ShouldInclude = (description) => description.GroupName == "weather";
                options.AddDocumentTransformer(new ApiInfoDocumentTransformer("Weather API", "2.0"));
            });

            builder.Services.AddOpenApi("weathertemperature", options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
                options.ShouldInclude = (description) => description.GroupName == "weathertemperature";
                options.AddDocumentTransformer(new ApiInfoDocumentTransformer("Weather Temperature API", "1.0"));
            });

            builder.Services.AddSingleton<IWeatherService, WeatherService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();                  // /openapi/v1.json
                app.MapScalarApiReference(scalarOptions =>
                {
                    scalarOptions.Title = "Samayas Weather APIs";
                    scalarOptions.Servers = [];
                    scalarOptions.Theme = ScalarTheme.None;
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
                        new ScalarDocument("weathertemperature", "Weather Temperature API v1", "/openapi/weathertemperature.json")
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