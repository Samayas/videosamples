using System.ComponentModel.DataAnnotations;

namespace OpenAPIV3.Contracts.Requests
{
    /// <summary>
    /// Query parameter for the weather temparature request.
    /// </summary>
    public class WeatherRequest
    {
        /// <summary>
        /// The name of the location to retrieve the weather temperature.
        /// </summary>
        /// <example>London</example>
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;
    }
}
