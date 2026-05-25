using BuildCleanup.ServiceAgents;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BuildCleanup
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddCommandLine(args)
                .Build();

            string pat = config["pat"] ?? string.Empty;
            string collectionUrl = config["collectionUrl"] ?? string.Empty;
            string projectName = config["projectName"] ?? string.Empty;
            string postFilter = config["postFilter"] ?? "WeeklyVersionCheck";

            if (string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(collectionUrl) || string.IsNullOrEmpty(projectName))
            {
                Console.WriteLine("Error: Missing required configuration.");
                Console.WriteLine("Usage: dotnet run --pat <token> --collectionUrl <url> --projectName <name>");
                Console.WriteLine("Or ensure they are defined in appsettings.json");
                return;
            }

            try
            {
                // Ensure default proxy is used
                WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
                WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

                Console.WriteLine($"Starting process for Project: {projectName}...");
                IList<int> definitionIds = await AzureDevopsServiceAgent.RetrieveAllBuildDefinitions(pat, collectionUrl, projectName, postFilter);

                foreach (int definitionId in definitionIds)
                {
                    await AzureDevopsServiceAgent.DeleteAllNonLastBuilds(pat, collectionUrl, projectName, definitionId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

