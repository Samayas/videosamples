using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Linq;

namespace OpenAPIV3.Infrastructure
{
    public class ApiInfoDocumentTransformer : IOpenApiDocumentTransformer
    {
        private readonly string Title;
        private readonly string Version;
        private readonly string Name;
        private readonly string Description;

        public ApiInfoDocumentTransformer(string title, string version, string name, string description)
        {
            this.Title = title;
            this.Version = version;
            this.Name = name;
            this.Description = description;
        }

        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Info = new OpenApiInfo
            {
                Title = this.Title,
                Version = this.Version,
                Description = $"{this.Title} - {this.Version}",
                Contact = new OpenApiContact
                {
                    Name = "Stef",
                    Email = "info@samayas.eu",
                    Url = new Uri("https://www.samayas.eu")
                },
                License = new OpenApiLicense() 
                {  
                    Url = new Uri("https://www.samayas.eu/")
                }
            };

            AddOrUpdateTag(document,this.Name, this.Description);

            return Task.CompletedTask;
        }

        private static void AddOrUpdateTag(OpenApiDocument document, string tagName, string description)
        {
            document.Tags ??= new HashSet<OpenApiTag>();

            OpenApiTag? existingTag = document.Tags.FirstOrDefault(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));

            if (existingTag is null)
            {
                document.Tags.Add(new OpenApiTag
                {
                    Name = tagName,
                    Description = description
                });

                return;
            }

            if (string.IsNullOrWhiteSpace(existingTag.Description))
            {
                existingTag.Description = description;
            }
        }
    }
}
