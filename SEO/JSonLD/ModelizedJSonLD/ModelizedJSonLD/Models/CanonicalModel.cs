namespace ModelizedCanonical.Models
{
    public class CanonicalModel
    {
        public CanonicalModel()
        {
        }

        public CanonicalModel(string siteUrl, string subPart)
        {
            this.SiteUrl = siteUrl;
            this.SubPart = subPart;
            this.HasCanonical = true;
        }

        public bool HasCanonical { get; set; }

        public string SubPart { get; set; } = string.Empty;

        public string SiteUrl { get; set; } = string.Empty;

        public string BuildCanonical()
        {
            return new Uri(this.SiteUrl + this.SubPart).ToString();
        }
    }
}
