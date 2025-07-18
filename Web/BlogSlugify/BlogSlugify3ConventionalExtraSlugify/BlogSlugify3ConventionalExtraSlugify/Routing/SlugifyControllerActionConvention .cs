using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BlogSlugify3ConventionalExtraSlugify.Routing
{
    public class SlugifyControllerActionConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (ControllerModel controller in application.Controllers)
            {
                string? controllerName = Slugify(controller.ControllerName);
                foreach (ActionModel action in controller.Actions)
                {
                    string? actionName = Slugify(action.ActionName);

                    if (actionName.ToLower() != action.ActionName.ToLower())
                    {
                        // Clear existing selectors to replace with custom route
                        action.Selectors.Clear();

                        action.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel(
                                new Microsoft.AspNetCore.Mvc.RouteAttribute($"{controllerName}/{actionName}")
                            )
                        });
                    }
                }
            }
        }

        private static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            return Regex.Replace(input, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
        }
    }
}
