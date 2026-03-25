using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OpenAPIV3.Infrastructure
{
    public class ApiInfoDocumentTransformer : IOpenApiDocumentTransformer
    {
        private readonly string Title;
        private readonly string Version;

        public ApiInfoDocumentTransformer(string title, string version)
        {
            this.Title = title;
            this.Version = version;
        }

        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Info = new OpenApiInfo
            {
                Title = this.Title,
                Version = this.Version,
                Description = $"{this.Title} - {this.Version}"
            };
            return Task.CompletedTask;
        }
    }
}
