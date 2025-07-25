using System.Xml.Linq;

using MergeCSProjectsToNuspec.Library.Models;

namespace MergeCSProjectsToNuspec.Library.Engine
{
    public static class CSProjPackageReferencesEngine
    {
        public static IList<PackageReference> GetAllPackageReferencesFromProjectFile(string projectFile)
        {
            IList<PackageReference> packageReferences = new List<PackageReference>();

            if (!File.Exists(projectFile))
            {
                return packageReferences;
            }

            XDocument doc = XDocument.Load(projectFile);
            if (doc == null)
            {
                return packageReferences;
            }

            IEnumerable<XElement> packageElements = doc.Descendants("PackageReference");
            if (packageElements != null)
            {
                foreach (XElement element in packageElements)
                {
                    string? packageName = element.Attribute("Include")?.Value;
                    string? packageVersion = element.Attribute("Version")?.Value;

                    if (packageVersion == null)
                    {
                        packageVersion = element.Element("Version")?.Value;
                    }

                    if (!string.IsNullOrEmpty(packageName))
                    {
                        packageReferences.Add(new PackageReference
                        {
                            PackageName = packageName,
                            PackageVersion = packageVersion
                        });
                    }
                }
            }

            return packageReferences;
        }
    }
}
