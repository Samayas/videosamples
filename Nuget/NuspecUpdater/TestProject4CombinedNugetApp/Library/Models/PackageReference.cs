namespace MergeCSProjectsToNuspec.Library.Models
{
    public class PackageReference
    {
        public string PackageName { get; set; } = string.Empty;
        public string PackageVersion { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Package Name: {PackageName} - Version: {PackageVersion}";
        }
    }
}
