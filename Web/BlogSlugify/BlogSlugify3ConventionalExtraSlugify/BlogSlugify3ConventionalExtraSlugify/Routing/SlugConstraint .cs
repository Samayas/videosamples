using System.Text.RegularExpressions;

namespace BlogSlugify3ConventionalExtraSlugify.Routing
{
    public class SlugConstraint : IRouteConstraint
    {
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out var value))
            {
                return false;
            }

            string? stringValue = Convert.ToString(value);
            return !string.IsNullOrEmpty(stringValue) && Regex.IsMatch(stringValue, "^[a-z0-9]+(?:-[a-z0-9]+)*$");
        }
    }
}
