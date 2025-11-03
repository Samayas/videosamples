namespace ModelizedCanonical.Models
{
    public class JSonLDModel
    {
        public bool HasJSonLD { get; set; }

        public JSonLDType Type { get; set; }

        public string Description { get; set; } = string.Empty;

        public string[] Images { get; set; } = null;

        public string Author { get; set; } = string.Empty;

        public JSonLDAuthorType AuthorType { get; set; }

        public string Publisher { get; set; } = string.Empty;

        public string PublisherOrganization { get; set; } = string.Empty;

        public string PublisherOrganizationLinkedIn { get; set; } = string.Empty;

        public string PublisherOrganizationX { get; set; } = string.Empty;

        public string PublisherLogo { get; set; } = string.Empty;

        public string PublisherLogoSize { get; set; } = string.Empty;

        public DateTime PublishDate { get; set; }

        public string Headline { get; set; } = string.Empty;
    }
}
