using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace BuildCleanup.ServiceAgents
{
    public static class AzureDevopsServiceAgent
    {
        public static async Task<IList<int>> RetrieveAllBuildDefinitions(string pat, string collectionUrl, string projectName, string postFilter)
        {
            IList<int> buildDefinitions = new List<int>();

            try
            {
                // Create Credentials
                VssBasicCredential credentials = new VssBasicCredential("", pat);
                VssConnection connection = new VssConnection(new Uri(collectionUrl), credentials);

                // Establish Connection
                await connection.ConnectAsync();

                // Build Client
                using (BuildHttpClient buildClient = await connection.GetClientAsync<BuildHttpClient>())
                {
                    // Get all build definitions (pipelines)
                    List<BuildDefinitionReference> definitions = await buildClient.GetDefinitionsAsync(project: projectName);
                    if (definitions == null || definitions.Count == 0)
                    {
                        return new List<int>();
                    }

                    // Filter by postFilter (case-insensitive)
                    IList<int> filteredDefinitionIds = definitions
                        .Where(d => d.Name != null && d.Name.EndsWith(postFilter, StringComparison.OrdinalIgnoreCase))
                        .Select(d => d.Id)
                        .ToList();

                    return filteredDefinitionIds;
                }
            }
            catch
            {
            }

            return buildDefinitions;
        }

        public static async Task DeleteAllNonLastBuilds(string pat, string collectionUrl, string projectName, int definitionId)
        {
            try
            {
                VssBasicCredential credentials = new VssBasicCredential("", pat);
                VssConnection connection = new VssConnection(new Uri(collectionUrl), credentials);

                await connection.ConnectAsync();

                using (BuildHttpClient buildClient = await connection.GetClientAsync<BuildHttpClient>())
                {
                    IList<Build>? builds = await buildClient.GetBuildsAsync(project: projectName, definitions: new[] { definitionId }, queryOrder: BuildQueryOrder.FinishTimeDescending);
                    if (builds == null || builds.Count == 0)
                    {
                        return;
                    }

                    Build? latest = builds.FirstOrDefault();

                    if (latest == null || builds.Count <= 1)
                    {
                        return;
                    }

                    foreach (Build build in builds.Skip(1))
                    {
                        try
                        {
                            IList<RetentionLease> leases = await buildClient.GetRetentionLeasesForBuildAsync(project: projectName, buildId: build.Id);
                            if (leases != null && leases.Count > 0)
                            {
                                IEnumerable<int> leaseIds = leases.Select(l => l.LeaseId);
                                await buildClient.DeleteRetentionLeasesByIdAsync(projectName, leaseIds);
                            }

                            // Might still be under retention.
                            await buildClient.DeleteBuildAsync(projectName, build.Id);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
