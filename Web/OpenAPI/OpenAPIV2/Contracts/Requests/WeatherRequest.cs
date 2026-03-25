using System.ComponentModel.DataAnnotations;

namespace OpenAPIV2.Contracts.Requests
{
    public class WeatherRequest
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;
    }
}
