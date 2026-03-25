using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OpenAPIV3.Contracts.Requests;
using OpenAPIV3.Contracts.Response;
using OpenAPIV3.Services.Interfaces;

namespace OpenAPIV3.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "weathertemperature")]
    [ApiVersion(1.0)]
    public class WeatherTemperatureController : ControllerBase
    {
        private readonly IWeatherService WeatherService;

        public WeatherTemperatureController(IWeatherService weatherService)
        {
            this.WeatherService = weatherService;
        }

        /// <summary>Gets the weather temperature for a given location.</summary>
        /// <param name="weatherRequest">The query parameters containing the target location.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A <see cref="WeatherResponse"/> with the current temperature.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<decimal>> Get([FromQuery] WeatherRequest weatherRequest)
        {
            // Invoke the service
            WeatherResponse forecast = await this.WeatherService.GetForecastAsync(weatherRequest.Location);

            // Return the forecast to the client
            return Ok(forecast.TemperatureCelsius);
        }
    }
}
