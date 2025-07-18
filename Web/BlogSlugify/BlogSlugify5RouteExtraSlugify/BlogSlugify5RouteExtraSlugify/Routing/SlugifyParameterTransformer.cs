﻿using System.Text.RegularExpressions;

namespace BlogSlugify5RouteExtraSlugify.Routing
{
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object value)
        {
            if (value == null)
            {
                return null;
            }

            // Slugify value
            string slugify = Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();

            return slugify;
        }
    }
}
