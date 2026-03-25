using Aspire.Hosting;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Add the ClamAV service
IResourceBuilder<ContainerResource> clamAv = builder
    .AddContainer("clamav", "clamav/clamav", "latest")
    .WithEndpoint(port: 3310, targetPort: 3310, name: "tcp", scheme: "tcp");

IResourceBuilder<ProjectResource> webApp = builder
    .AddProject<UploadAndScan2Full>("uploadapp")
    .WithReference(clamAv.GetEndpoint("tcp"))  // passes EndpointReference, not the resource
    .WaitFor(clamAv);

builder.Build().Run();