using OpenAPIV3.Contracts.Response;

namespace OpenAPIV3.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherResponse> GetForecastAsync(string location);
    }
}
