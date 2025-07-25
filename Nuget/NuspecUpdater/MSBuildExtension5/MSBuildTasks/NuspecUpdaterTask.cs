using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MergeCSProjectsToNuspec.Library.Models;
using MergeCSProjectsToNuspec.Library.ExtensionMethods;
using MSBuildTasks.Engine;

namespace MSBuildTasks
{
    public class NuspecUpdaterTask : Task
    {
        [Required]
        public string NuspecFilePath { get; set; }

        [Required]
        public string ProjectFiles { get; set; }


        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Starting custom post-processing task2...");

            IList<PackageReference> allPackageReferences = new List<PackageReference>();
            string[] projectFilePaths = ProjectFiles.Split(';');

            Log.LogMessage(MessageImportance.High, "Processing Project Files:");
            foreach (string projectFile in projectFilePaths)
            {
                Log.LogMessage(MessageImportance.Normal, "-> Reading project: {0}", projectFile.Trim());
                IList<PackageReference> projectPackages = CSProjPackageReferencesEngine.GetAllPackageReferencesFromProjectFile(projectFile.Trim(), Log);
                allPackageReferences.Merge(projectPackages);

                foreach (PackageReference packageReference in allPackageReferences)
                {
                    Log.LogMessage(MessageImportance.Normal, "-> Found Package reference: {0}", packageReference);
                }
            }

            Log.LogMessage(MessageImportance.High, "Updating Nuspec file: {0}", NuspecFilePath);
            NuspecUpdatePackageReferencesEngine.GetAllPackageReferencesFromProjectFile(NuspecFilePath, allPackageReferences, Log);

            // If Log.HasLoggedErrors is true, the task will fail.
            if (Log.HasLoggedErrors)
            {
                Log.LogMessage(MessageImportance.High, "Merge CS Projects to Nuspec task finished with errors.");
            }
            else
            {
                Log.LogMessage(MessageImportance.High, "Finished Merge CS Projects to Nuspec task successfully.");
            }

            return !Log.HasLoggedErrors;
        }
    }
}
