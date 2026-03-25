using OpenAPIV2.Contracts.Response;

namespace OpenAPIV2.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherResponse> GetForecastAsync(string location);
    }
}
