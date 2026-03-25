using System.ComponentModel.DataAnnotations;

namespace UploadAndScan2Full.Models
{
    public class UploadViewModel
    {
        [Display(Name = "Files")]
        [Required(ErrorMessage = "At least one file is required")]
        public IList<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
