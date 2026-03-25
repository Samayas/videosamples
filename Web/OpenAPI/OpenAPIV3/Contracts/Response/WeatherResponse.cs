namespace OpenAPIV3.Contracts.Response
{
    public class WeatherResponse
    {
        /// <summary>The date of the forecast.</summary>
        /// <example>2026-03-25T00:00:00</example>
        public DateTime Date { get; set; }

        /// <summary>Temperature in Celsius.</summary>
        /// <example>18.5</example>
        public decimal TemperatureCelsius { get; set; }

        /// <summary>A short description of weather conditions.</summary>
        /// <example>Partly cloudy</example>
        public string Summary { get; set; } = string.Empty;
    }
}
