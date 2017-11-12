using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Linoa.ActiveRouteTagHelper
{
    [HtmlTargetElement("a", Attributes = "is-active-route,asp-controller,asp-action")]
    [OutputElementHint("a")]
    public class ActiveRouteTagHelper : TagHelper
    {
        private const string ActiveRouteAttributeName = "is-active-route";

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; }

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; }

        private IDictionary<string, string> _routeValues;
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<string, string> RouteValues
        {
            get => _routeValues ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            set => _routeValues = value;
        }

        private static string _isActiveClass;
        [HtmlAttributeName(ActiveRouteAttributeName)]
        public string IsActiveClass
        {
            get => _isActiveClass ?? "active";
            set => _isActiveClass = value;
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (IsCurrentRoute())
            {
                AddActiveClass(output);
            }

            output.Attributes.RemoveAll(ActiveRouteAttributeName);
        }

        private bool IsCurrentRoute()
        {
            var currentController = ViewContext.RouteData.Values["Controller"].ToString();
            var currentAction = ViewContext.RouteData.Values["Action"].ToString();

            if (!string.IsNullOrWhiteSpace(Controller)
                && !Controller.Equals(currentController, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Action)
                && !Action.Equals(currentAction, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            foreach (var routeValue in RouteValues)
            {
                if (!ViewContext.RouteData.Values.ContainsKey(routeValue.Key) ||
                    ViewContext.RouteData.Values[routeValue.Key].ToString() != routeValue.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void AddActiveClass(TagHelperOutput output)
        {
            var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttr == null)
            {
                classAttr = new TagHelperAttribute("class", IsActiveClass);
                output.Attributes.Add(classAttr);
            }
            else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf(IsActiveClass) < 0)
            {
                output.Attributes.SetAttribute("class", classAttr.Value == null
                    ? IsActiveClass
                    : classAttr.Value.ToString() + " " + IsActiveClass);
            }
        }
    }
}
