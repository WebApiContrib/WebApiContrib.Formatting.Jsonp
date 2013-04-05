WebApiContrib.Formatting.Jsonp
==============================

`WebApiContrib.Formatting.Jsonp` provides a [JSONP](https://en.wikipedia.org/wiki/JSONP) [MediaTypeFormatter](http://msdn.microsoft.com/en-us/library/system.net.http.formatting.mediatypeformatter.aspx) implementation for [ASP.NET Web API](http://www.asp.net/web-api).

In order to add it to your Web API solution, run `Install-Package WebApiContrib.Formatting.Jsonp` from your NuGet Package Manager console in Visual Studio.

To use the `JsonpMediaTypeFormatter`, add the following code to your Web API Configuration:

`FormatterConfig.RegisterFormatters(GlobalConfiguration.Configuration.Formatters);`

The `FormatterConfig` class looks this:

    public class FormatterConfig
    {
        public static void RegisterFormatters(MediaTypeFormatterCollection formatters)
        {
            var jsonFormatter = formatters.JsonFormatter;
            jsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Insert the JSONP formatter in front of the standard JSON formatter.
            var jsonpFormatter = new JsonpMediaTypeFormatter(formatters.JsonFormatter);
            formatters.Insert(0, jsonpFormatter);
        }
    }

After that, update your Default ASP.NET Web API route in `/App_Start/RouteConfig.cs`:

    routes.MapHttpRoute(
        name: "DefaultApi",
        routeTemplate: "api/{controller}/{id}/{format}",
        defaults: new { id = RouteParameter.Optional, format = RouteParameter.Optional }
    );

Now you should be able to issue JSONP requests against your Web API.

To see the `JsonpMediaTypeFormatter` in action, just clone this project, run the `WebContribContrib.Formatting.Jsonp.SampleWebHost` project web application, and then start the `WebApiContrib.Formatting.Jsonp.SampleJQueryClient` web application and hit the "Get JSONP" button.

