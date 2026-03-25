using System.ComponentModel.DataAnnotations;

namespace OpenAPIV1.Contracts.Requests
{
    public class WeatherRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;
    }
}
