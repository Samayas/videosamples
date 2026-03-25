using OpenAPIV1.Contracts.Response;
using OpenAPIV1.Services.Interfaces;
using System.Text.Json;

namespace OpenAPIV1.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient HttpClient;
        private readonly string ApiKey = string.Empty;
        private readonly string BaseUrl = string.Empty;

        public WeatherService(IConfiguration configuration)
        {
            this.ApiKey = configuration["WeatherApi:ApiKey"] ?? string.Empty;
            this.BaseUrl = configuration["WeatherApi:BaseUrl"] ?? string.Empty;
            this.HttpClient = new HttpClient();
        }

        public async Task<WeatherResponse> GetForecastAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException("Location must not be null or empty.", nameof(location));
            }

            // Build request URL
            string requestUrl = $"{this.BaseUrl}/forecast.json?q={Uri.EscapeDataString(location)}&key={this.ApiKey}&aqi=no&alerts=no";

            // Send GET request
            HttpResponseMessage response = await this.HttpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            // Read response body
            string json = await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            string localTime = root.GetProperty("location").GetProperty("localtime").GetString() ?? string.Empty;
            double temperatureCelsius = root.GetProperty("current").GetProperty("temp_c").GetDouble();
            string summary = root.GetProperty("current").GetProperty("condition").GetProperty("text").GetString() ?? string.Empty;

            return new WeatherResponse
            {
                Date = DateTime.TryParse(localTime, out DateTime parsed) ? parsed : DateTime.UtcNow,
                TemperatureCelsius = (decimal)temperatureCelsius,
                Summary = summary
            };
        }
    }
}
