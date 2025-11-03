using System;
using System.Globalization;
using System.Text.Json;

using System.Text.Json.Serialization;

namespace ModelizedCanonical.Models.Helpers
{
    /// <summary>
    /// JSon LD Article Bulder
    /// </summary>
    public static class JsonLDArticleBuilder
    {
        public static string BuildJsonLD(JSonLDModel jsonLDModel, CanonicalModel canonical, string pageTitle, string? inLanguage = "en")
        {
            // Build the full page URL from siteUrl + canonical path
            string pageUrl = BuildFullPageUrl(canonical.SiteUrl, canonical);

            switch (jsonLDModel.Type)
            {
                case JSonLDType.HomePage:
                case JSonLDType.WebPage:
                    return BuildWebPageGraph(jsonLDModel, canonical.SiteUrl, pageUrl, pageTitle, inLanguage);

                case JSonLDType.Person:
                    return BuildPersonJSonLD(jsonLDModel, pageUrl, inLanguage);

                case JSonLDType.Organization:
                    return BuildOrganizationJSonLD(jsonLDModel, canonical.SiteUrl, inLanguage);

                case JSonLDType.WebSite:
                    return BuildWebSiteJSonLD(jsonLDModel, canonical.SiteUrl, inLanguage);

                case JSonLDType.FAQPage:
                case JSonLDType.HowTo:
                case JSonLDType.VideoObject:
                    return BuildGenericWebPageFallback(jsonLDModel, canonical.SiteUrl, pageUrl, pageTitle, inLanguage);

                case JSonLDType.Article:
                case JSonLDType.BlogPosting:
                case JSonLDType.NewsArticle:
                case JSonLDType.TechArticle:
                case JSonLDType.Report:
                default:
                    // Default to Article family
                    return BuildArticleJSonLD(jsonLDModel, canonical, inLanguage);
            }        
        }

        private static string BuildFullPageUrl(string siteUrl, CanonicalModel canonical)
        {
            string baseUrl = siteUrl.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(canonical.SubPart))
            {
                return baseUrl;
            }

            string path = canonical.SubPart.TrimStart('/');

            return $"{baseUrl}/{path}";
        }

        private static string BuildWebPageGraph(JSonLDModel jsonLDModel, string siteUrl, string pageUrl, string pageTitle, string? inLanguage)
        {
            string? siteRoot = GetSiteRoot(siteUrl);
            string baseId = siteUrl.TrimEnd('/');

            // WebSite node
            IDictionary<string, object> webSite = new Dictionary<string, object>
            {
                { "@type", "WebSite" },
                { "@id", $"{baseId}#website"},
                { "url", siteRoot ?? string.Empty },
                { "name", GetFirstNonEmpty(jsonLDModel.PublisherOrganization, jsonLDModel.Publisher, pageTitle, "Website") }
            };

            if (!string.IsNullOrWhiteSpace(inLanguage))
            {
                webSite["inLanguage"] = inLanguage;
            }

            // Organization node as publisher (optional)
            IDictionary<string, object>? organizationNode = BuildOrganizationNodeFromModel(jsonLDModel);
            if (organizationNode != null)
            {
                organizationNode["@id"] = $"{baseId}#org";
                webSite["publisher"] = new Dictionary<string, object> { { "@id", $"{baseId}#org" } };
            }

            // WebPage node
            string webPageId = pageUrl.TrimEnd('/');
            IDictionary<string, object> webPage = new Dictionary<string, object>
            {
                { "@type", "WebPage" },
                { "@id", $"{webPageId}#webpage" },
                { "url", pageUrl },
                { "name", GetFirstNonEmpty(pageTitle, jsonLDModel.Headline, jsonLDModel.Description, "Page") },
                { "isPartOf", new Dictionary<string, object> { { "@id", $"{baseId}#website" } } },
                { "description", GetFirstNonEmpty(jsonLDModel.Description, string.Empty) }
            };

            if (!string.IsNullOrWhiteSpace(inLanguage))
            {
                webPage["inLanguage"] = inLanguage;
            }

            // Optional primary images on the page
            IList<string> images = NormalizeImages(jsonLDModel.Images);
            if (images.Count > 0)
            {
                webPage["image"] = images;
            }

            // Build @graph
            IList<object> graph = new List<object> { webPage, webSite };
            if (organizationNode != null)
            {
                graph.Add(organizationNode);
            }

            IDictionary<string, object> root = new Dictionary<string, object>
            {
                { "@context", "https://schema.org" },
                { "@graph", graph }
            };

            return WrapAsScript(root);
        }

        private static string BuildArticleJSonLD(JSonLDModel jsonLDModel, CanonicalModel canonical, string? inLanguage)
        {
            string schemaType = MapArticleType(jsonLDModel);

            IDictionary<string, object> root = new Dictionary<string, object>
            {
                { "@context", "https://schema.org" },
                { "@type", schemaType },
                { "mainEntityOfPage", new Dictionary<string, object> { { "@type", "WebPage" }, { "@id", canonical.SiteUrl } } },
                { "headline", GetFirstNonEmpty(jsonLDModel.Headline, string.Empty) },
                { "description", GetFirstNonEmpty(jsonLDModel.Description, string.Empty) }
            };

            IList<string> images = NormalizeImages(jsonLDModel.Images);
            if (images.Count > 0)
            {
                root["image"] = images;
            }

            if (!string.IsNullOrWhiteSpace(inLanguage))
            {
                root["inLanguage"] = inLanguage;
            }

            IDictionary<string, object>? authorNode = BuildAuthorNode(jsonLDModel);
            if (authorNode != null)
            {
                root["author"] = authorNode;
            }

            IDictionary<string, object>? publisherNode = BuildPublisherNode(jsonLDModel);
            if (publisherNode != null)
            {
                root["publisher"] = publisherNode;
            }

            if (jsonLDModel.PublishDate != default(DateTime))
            {
                string iso = ToISO8601(jsonLDModel.PublishDate);
                root["datePublished"] = iso;
                root["dateModified"] = iso;
            }

            if (!string.IsNullOrWhiteSpace(canonical.SubPart))
            {
                root["identifier"] = canonical.SubPart.Trim();
            }

            return WrapAsScript(root);
        }

        private static string BuildPersonJSonLD(JSonLDModel jsonLDModel, string pageUrl, string? inLanguage)
        {
            IDictionary<string, object> person = new Dictionary<string, object>
            {
                { "@context", "https://schema.org" },
                { "@type", "Person" }
            };

            // Reuse model.Author as "Name|Url"
            if (!string.IsNullOrWhiteSpace(jsonLDModel.Author))
            {
                string[] parts = jsonLDModel.Author.Split('|', StringSplitOptions.RemoveEmptyEntries);
                string? name = parts.Length > 0 ? parts[0].Trim() : null;
                string? url = parts.Length > 1 ? parts[1].Trim() : null;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    person["name"] = name;
                }

                if (!string.IsNullOrWhiteSpace(url))
                {
                    person["url"] = url;
                }
            }

            if (!string.IsNullOrWhiteSpace(inLanguage))
            {
                person["inLanguage"] = inLanguage;
            }

            person["mainEntityOfPage"] = new Dictionary<string, object>
            {
                { "@type", "WebPage" },
                { "@id", pageUrl }
            };

            return WrapAsScript(person);
        }

        private static string BuildOrganizationJSonLD(JSonLDModel jsonLDModel, string siteUrl, string? inLanguage)
        {
            IDictionary<string, object> org = BuildOrganizationNodeFromModel(jsonLDModel) ?? new Dictionary<string, object> { { "@type", "Organization" } };

            if (!org.ContainsKey("url"))
            {
                org["url"] = GetSiteRoot(siteUrl) ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(inLanguage))
            {
                org["inLanguage"] = inLanguage;
            }

            return WrapAsScript(new Dictionary<string, object>
            {
                { "@context", "https://schema.org" },
                { "@graph", new List<object> { org } }
            });
        }

        private static string BuildWebSiteJSonLD(JSonLDModel jsonLDModel, string siteUrl, string? inLanguage)
        {
            string? siteRoot = GetSiteRoot(siteUrl);
            Dictionary<string, object> webSite = new Dictionary<string, object>
            {
                { "@type", "WebSite" },
                { "url", GetFirstNonEmpty(siteRoot, string.Empty) },
                { "name", GetFirstNonEmpty(jsonLDModel.PublisherOrganization, jsonLDModel.Publisher, jsonLDModel.Headline, "Website") }
            };

            if (!string.IsNullOrWhiteSpace(inLanguage))
            {
                webSite["inLanguage"] = inLanguage;
            }

            IDictionary<string, object>? organizationNode = BuildOrganizationNodeFromModel(jsonLDModel);
            IList<object> graph = organizationNode != null ? new List<object> { webSite, organizationNode } : new List<object> { webSite };

            return WrapAsScript(new Dictionary<string, object>
            {
                { "@context", "https://schema.org" },
                { "@graph", graph }
            });
        }

        private static string BuildGenericWebPageFallback(JSonLDModel jsonLDModel, string siteUrl, string pageUrl, string pageTitle, string? inLanguage)
        {
            // Minimal WebPage with optional WebSite linkage
            string? siteRoot = GetSiteRoot(siteUrl);
            string baseId = siteUrl.TrimEnd('/');

            IDictionary<string, object> webSite = new Dictionary<string, object>
            {
                { "@type", "WebSite" },
                { "@id", $"{baseId}#website" },
                { "url", GetFirstNonEmpty(siteRoot, string.Empty) },
                { "name", GetFirstNonEmpty(jsonLDModel.PublisherOrganization, jsonLDModel.Publisher, pageTitle, "Website") }
            };

            string webPageId = pageUrl.TrimEnd('/');
            IDictionary<string, object> webPage = new Dictionary<string, object>
            {
                { "@type", "WebPage" },
                { "@id", $"{webPageId}#webpage"  },
                { "url", pageUrl },
                { "name", GetFirstNonEmpty(pageTitle, jsonLDModel.Headline, jsonLDModel.Description, "Page") },
                { "isPartOf", new Dictionary<string, object> { { "@id", $"{baseId}#website" } } },
                { "description", GetFirstNonEmpty(jsonLDModel.Description, string.Empty) }
            };

            if (!string.IsNullOrWhiteSpace(inLanguage))
            {
                webPage["inLanguage"] = inLanguage;
                webSite["inLanguage"] = inLanguage;
            }

            IList<object> graph = new List<object> { webPage, webSite };

            return WrapAsScript(new Dictionary<string, object>
            {
                { "@context", "https://schema.org" },
                { "@graph", graph }
            });
        }

        private static string MapArticleType(JSonLDModel jsonLDModel)
        {
            // Maps JSonLDType to schema.org type for articles
            string name = jsonLDModel.Type.ToString();
            if (string.Equals(name, "Article", StringComparison.OrdinalIgnoreCase))
            {
                return "Article";
            }

            if (string.Equals(name, "BlogPosting", StringComparison.OrdinalIgnoreCase))
            {
                return "BlogPosting";
            }

            if (string.Equals(name, "NewsArticle", StringComparison.OrdinalIgnoreCase))
            {
                return "NewsArticle";
            }

            if (string.Equals(name, "TechArticle", StringComparison.OrdinalIgnoreCase))
            {
                return "TechArticle";
            }

            if (string.Equals(name, "Report", StringComparison.OrdinalIgnoreCase))
            {
                return "Report";
            }

            return "BlogPosting";
        }

        private static IDictionary<string, object>? BuildAuthorNode(JSonLDModel jsonLDModel)
        {
            if (string.IsNullOrWhiteSpace(jsonLDModel.Author))
            {
                return null;
            }

            string[] parts = jsonLDModel.Author.Split('|', StringSplitOptions.RemoveEmptyEntries);
            string? name = parts.Length > 0 ? parts[0].Trim() : null;
            string? url = parts.Length > 1 ? parts[1].Trim() : null;

            string type = jsonLDModel.AuthorType == JSonLDAuthorType.Organization ? "Organization" : "Person";

            IDictionary<string, object> node = new Dictionary<string, object>
            {
                { "@type", type }
            };

            if (!string.IsNullOrWhiteSpace(name))
            {
                node["name"] = name;
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                node["url"] = url;
            }

            return node;
        }

        private static IDictionary<string, object>? BuildPublisherNode(JSonLDModel jsonLDModel)
        {
            string name = !string.IsNullOrWhiteSpace(jsonLDModel.PublisherOrganization) ? jsonLDModel.PublisherOrganization : jsonLDModel.Publisher;

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(jsonLDModel.PublisherLogo))
            {
                return null;
            }

            IDictionary<string, object> publishierNode = new Dictionary<string, object>
            {
                { "@type", "Organization" }
            };

            if (!string.IsNullOrWhiteSpace(name))
            {
                publishierNode["name"] = name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(jsonLDModel.PublisherLogo))
            {
                int? width;
                int? height;
                ParseLogoSizeCommaSeparated(jsonLDModel.PublisherLogoSize, out width, out height);

                Dictionary<string, object> logo = new Dictionary<string, object>
                {
                    { "@type", "ImageObject" },
                    { "url", jsonLDModel.PublisherLogo.Trim() }
                };

                if (width.HasValue) logo["width"] = width.Value;
                if (height.HasValue) logo["height"] = height.Value;

                publishierNode["logo"] = logo;
            }

            return publishierNode;
        }

        private static IDictionary<string, object>? BuildOrganizationNodeFromModel(JSonLDModel jsonLDModel)
        {
            string name = !string.IsNullOrWhiteSpace(jsonLDModel.PublisherOrganization) ? jsonLDModel.PublisherOrganization : jsonLDModel.Publisher;

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(jsonLDModel.PublisherLogo))
            {
                return null;
            }

            IDictionary<string, object> organizationNode = new Dictionary<string, object>
            {
                { "@type", "Organization" }
            };

            if (!string.IsNullOrWhiteSpace(name))
            {
                organizationNode["name"] = name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(jsonLDModel.PublisherLogo))
            {
                int? width;
                int? height;
                ParseLogoSizeCommaSeparated(jsonLDModel.PublisherLogoSize, out width, out height);

                Dictionary<string, object> logo = new Dictionary<string, object>
                {
                    { "@type", "ImageObject" },
                    { "url", jsonLDModel.PublisherLogo.Trim() }
                };

                if (width.HasValue) logo["width"] = width.Value;
                if (height.HasValue) logo["height"] = height.Value;

                organizationNode["logo"] = logo;
            }

            // Add sameAs array for social media profiles
            List<string> sameAsList = new List<string>();

            if (!string.IsNullOrWhiteSpace(jsonLDModel.PublisherOrganizationLinkedIn))
            {
                sameAsList.Add(jsonLDModel.PublisherOrganizationLinkedIn.Trim());
            }

            if (!string.IsNullOrWhiteSpace(jsonLDModel.PublisherOrganizationX))
            {
                sameAsList.Add(jsonLDModel.PublisherOrganizationX.Trim());
            }

            if (sameAsList.Count > 0)
            {
                organizationNode["sameAs"] = sameAsList;
            }

            return organizationNode;
        }

        private static IList<string> NormalizeImages(string[] images)
        {
            IList<string> result = new List<string>();
            if (images == null || images.Length == 0)
            {
                return result;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int count = 0; count < images.Length; count++)
            {
                string item = images[count];
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                string url = item.Trim();
                if (!seen.Contains(url))
                {
                    seen.Add(url);

                    result.Add(url);
                }
            }

            return result;
        }

        private static void ParseLogoSizeCommaSeparated(string size, out int? width, out int? height)
        {
            width = null;
            height = null;

            if (string.IsNullOrWhiteSpace(size))
            {
                return;
            }

            string normalized = size.Trim()
                                    .ToLowerInvariant()
                                    .Replace("px", string.Empty)
                                    .Replace(" ", string.Empty);

            string[] parts = normalized.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return;
            }

            if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int w))
            {
                width = w;
            }

            if (int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int h))
            {
                height = h;
            }
        }

        private static string ToISO8601(DateTime dateTime)
        {
            DateTimeOffset dateTimeOffset = dateTime.Kind == DateTimeKind.Unspecified ? new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Local)) : new DateTimeOffset(dateTime);

            return dateTimeOffset.ToString("o", CultureInfo.InvariantCulture);
        }

        private static string? GetSiteRoot(string absoluteUrl)
        {
            if (string.IsNullOrWhiteSpace(absoluteUrl))
            {
                return null;
            }

            if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out Uri? uri))
            {
                return absoluteUrl;
            }

            string root = uri.GetLeftPart(UriPartial.Authority);

            return root + "/";
        }

        private static string WrapAsScript(object obj)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            string json = JsonSerializer.Serialize(obj, options);
            string script = "<script type=\"application/ld+json\">" + Environment.NewLine + json + Environment.NewLine + "</script>";

            return script;
        }

        private static string GetFirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return values[values.Length - 1];
        }
    }
}
