using System.ComponentModel.DataAnnotations;

namespace UploadAndScan1Upload.Models
{
    public class UploadViewModel
    {
        [Required(ErrorMessage = "Please enter the User Name.")]
        [Display(Name = "User Name")]
        public string UploadedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a confidentiality level.")]
        [Display(Name = "Confidentiality")]
        public string Confidentiality { get; set; } = "Public";

        [Display(Name = "Files")]
        [Required(ErrorMessage = "At least one file is required")]
        public IList<IFormFile> Files { get; set; } = new List<IFormFile>();
    }
}
