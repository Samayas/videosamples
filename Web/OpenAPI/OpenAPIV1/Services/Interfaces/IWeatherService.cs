using OpenAPIV1.Contracts.Response;

namespace OpenAPIV1.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherResponse> GetForecastAsync(string location);
    }
}
