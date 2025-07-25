using MergeCSProjectsToNuspec.Library.Engine;
using MergeCSProjectsToNuspec.Library.ExtensionMethods;
using MergeCSProjectsToNuspec.Library.Models;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Usage();
                return;
            }

            Console.WriteLine("Starting Merge CS Projects to Nuspec");
            Console.WriteLine("");

            string? nuspecFilePath = ParseArgument(args, "nuspec");
            string? projectFiles = ParseArgument(args, "projects");

            if (nuspecFilePath == null || projectFiles == null)
            {
                Usage();
                return;
            }

            Console.WriteLine($"  Nuspec File:{nuspecFilePath}");

            IList<PackageReference> packageReferences = new List<PackageReference>();
            IList<string> projectFilesList = new List<string>(projectFiles.Split(';', StringSplitOptions.RemoveEmptyEntries));

            Console.WriteLine("  Project Files:");
            foreach (string projectFile in projectFilesList)
            {
                Console.WriteLine($"  Project File:{projectFile}");

                IList<PackageReference> packageReference = CSProjPackageReferencesEngine.GetAllPackageReferencesFromProjectFile(projectFile);
                packageReferences.Merge(packageReference);  
            }

            Console.WriteLine("");
            Console.WriteLine($"  Updating nuspec file: {nuspecFilePath}");
            NuspecUpdatePackageReferencesEngine.GetAllPackageReferencesFromProjectFile(nuspecFilePath, packageReferences);

            Console.WriteLine("");
            Console.WriteLine("Finished Merge CS Projects to Nuspec");
        }

        private static string? ParseArgument(string[] args, string parameterName)
        {
            foreach (string arg in args)
            {
                string[] parts = arg.Split('=', 2);
                if (parts.Length == 2 && parts[0].Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                {
                    return parts[1];
                }
            }

            return null;
        }

        private static void Usage()
        {
            Console.WriteLine("Example:");
            Console.WriteLine(@"  MergeCSProjectsToNuspec.App.exe nuspec=file.nuspec projects=project1.csproj;project2.csproj");
            return;
        }
    }
}
