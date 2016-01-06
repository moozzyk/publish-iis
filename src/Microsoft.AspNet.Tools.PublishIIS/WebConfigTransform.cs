using System;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.AspNet.Tools.PublishIIS
{
    public static class WebConfigTransform
    {
        public static XDocument Transform(XDocument webConfig, string appName)
        {
            webConfig = webConfig == null || webConfig.Root.Name.LocalName != "configuration"
                ? XDocument.Parse("<configuration />")
                : webConfig;

            var webServerSection = GetOrCreateChild(webConfig.Root, "system.webServer");

            TransformHandlers(GetOrCreateChild(webServerSection, "handlers"));
            TransformHttpPlatform(GetOrCreateChild(webServerSection, "httpPlatform"), appName);

            return webConfig;
        }

        private static void TransformHandlers(XElement handlersElement)
        {
            var platformHandlerElement =
                handlersElement.Elements("add")
                    .FirstOrDefault(e => string.Equals((string)e.Attribute("name"), "httpplatformhandler", StringComparison.OrdinalIgnoreCase));

            if (platformHandlerElement == null)
            {
                platformHandlerElement = new XElement("add");
                handlersElement.Add(platformHandlerElement);
            }

            platformHandlerElement.SetAttributeValue("name", "httpPlatformHandler");
            platformHandlerElement.SetAttributeValue("path", (string)platformHandlerElement.Attribute("path") ?? "*");
            platformHandlerElement.SetAttributeValue("verb", (string)platformHandlerElement.Attribute("verb") ?? "*");
            platformHandlerElement.SetAttributeValue("modules", "httpPlatformHandler");
            platformHandlerElement.SetAttributeValue("resourceType", "Unspecified");
        }

        private static void TransformHttpPlatform(XElement httpPlatformElement, string appName)
        {
            // TODO: executable name should be passed as a param
            httpPlatformElement.SetAttributeValue("processPath", @"..\approot\${appName}");
            SetAttributeValueIfEmpty(httpPlatformElement, "stdoutLogEnabled", "false");
            SetAttributeValueIfEmpty(httpPlatformElement, "verstartupTimeLimit", "3600");
        }

        private static XElement GetOrCreateChild(XElement parent, string childName)
        {
            var childElement = parent.Element(childName);
            if (childElement == null)
            {
                childElement = new XElement(childName);
                parent.Add(childElement);
            }
            return childElement;
        }

        private static void SetAttributeValueIfEmpty(XElement element, string attributeName, string value)
        {
            element.SetAttributeValue(attributeName, (string)element.Attribute(attributeName) ?? value);
        }
    }
}