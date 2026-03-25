using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OpenAPIV2.Contracts.Requests;
using OpenAPIV2.Contracts.Response;
using OpenAPIV2.Services.Interfaces;

namespace OpenAPIV2.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "weather")]
    [ApiVersion(2.0)]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService WeatherService;

        public WeatherController(IWeatherService weatherService)
        {
            this.WeatherService = weatherService;
        }

        /// <summary>Gets the weather forecast for a given location.</summary>
        /// <param name="weatherRequest">The query parameters containing the target location.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A <see cref="WeatherResponse"/> with the current forecast.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<WeatherResponse>> Get([FromQuery] WeatherRequest weatherRequest)
        {
            if (string.IsNullOrWhiteSpace(weatherRequest.Location))
            {
                return BadRequest(new ProblemDetails { Title = "Location is required." });
            }

            // Invoke the service
            WeatherResponse forecast = await this.WeatherService.GetForecastAsync(weatherRequest.Location);

            if (forecast == null)
            {
                return NotFound();
            }

            // Return the forecast to the client
            return Ok(forecast);
        }
    }
}
