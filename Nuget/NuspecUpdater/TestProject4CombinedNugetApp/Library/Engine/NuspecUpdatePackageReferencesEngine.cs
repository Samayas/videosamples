using System;
using System.Xml.Linq;

using MergeCSProjectsToNuspec.Library.Models;

namespace MergeCSProjectsToNuspec.Library.Engine
{
    public static class NuspecUpdatePackageReferencesEngine
    {
        public static void GetAllPackageReferencesFromProjectFile(string nuspecFile, IList<PackageReference> packageReferences)
        {
            const string nuspecNamespace = "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd";
            XDocument document = XDocument.Load(nuspecFile);

            XElement? root = document.Root;
            if (root == null)
            {
                return;
            }

            XElement? metadata = root.Element(XName.Get("metadata", nuspecNamespace));
            if (metadata == null)
            {
                return;
            }

            XElement? dependencies = metadata.Element(XName.Get("dependencies", nuspecNamespace));
            if (dependencies == null)
            {
                dependencies = new XElement(XName.Get("dependencies", nuspecNamespace));
                metadata.Add(dependencies);
            }

            XElement? group = dependencies.Element(XName.Get("group", nuspecNamespace));
            if (group == null)
            {
                group = new XElement(XName.Get("group", nuspecNamespace));
                dependencies.Add(group);
            }

            Dictionary<string, XElement> existingDependencies = group
                .Elements(XName.Get("dependency", nuspecNamespace))
                .ToDictionary(
                    d => d.Attribute("id")?.Value ?? string.Empty,
                    d => d,
                    StringComparer.OrdinalIgnoreCase
                );

            foreach (PackageReference packageReference in packageReferences)
            {
                if (string.IsNullOrWhiteSpace(packageReference.PackageName))
                {
                    continue;
                }

                if (existingDependencies.TryGetValue(packageReference.PackageName, out XElement existingDependency))
                {
                    existingDependency.SetAttributeValue("version", packageReference.PackageVersion);
                }
                else
                {
                    XElement newDependency = new XElement(
                        XName.Get("dependency", nuspecNamespace),
                        new XAttribute("id", packageReference.PackageName),
                        new XAttribute("version", packageReference.PackageVersion));

                    group.Add(newDependency);
                }
            }

            document.Save(nuspecFile);
        }
    }
}
