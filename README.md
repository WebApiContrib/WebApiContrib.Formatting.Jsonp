WebApiContrib.Formatting.Jsonp
==============================

`WebApiContrib.Formatting.Jsonp` provides a [JSONP](https://en.wikipedia.org/wiki/JSONP) [MediaTypeFormatter](http://msdn.microsoft.com/en-us/library/system.net.http.formatting.mediatypeformatter.aspx) implementation for [ASP.NET Web API](http://www.asp.net/web-api).

In order to add it to your Web API solution, run `Install-Package WebApiContrib.Formatting.Jsonp` from your NuGet Package Manager console in Visual Studio.

To use the `JsonpMediaTypeFormatter`, add the following code to your configuration in Global.asax.cs:

`FormatterConfig.RegisterFormatters(GlobalConfiguration.Configuration.Formatters);`

The `FormatterConfig` class (typically in `/App_Start/FormatterConfig.cs`) looks this:

    public class FormatterConfig
    {
        public static void RegisterFormatters(MediaTypeFormatterCollection formatters)
        {
            var jsonFormatter = formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var jsonpFormatter = new JsonpMediaTypeFormatter(jsonFormatter, /* default */ callbackQueryParameter = "callback");
            formatters.Add(jsonpFormatter);
        }
    }

By default, you must specify `text/javascript` as the media type to accept. If you leave this out, you will likely send back `application/json` and find errors in your browser console. In addition to the standard `Accept` header, the `JsonpMediaTypeFormatter` allows query string and URI path extension mapping. To use the query string mapping, add `?format=jsonp&callback=?` to your request URI. (NOTE: you need to specify the `callback=?`, or whatever you name your callback, in the query string anyway.)

** NOTE: URI path extension is currently not working. **
URI path extension mapping uses the routing mechanisms. If you are using Attribute Routing, you should add "/{format}" after each route if you plan to use the URI mapping for jsonp, e.g. `[Route("api/value/{id:int}/{format?}")]`. If you will require the `Content-Type` header to specify `text/javascript`, then you can leave your routes alone. (See the sample applications for examples.)

If you are using traditional routing, update your Default ASP.NET Web API route in `/App_Start/WebApiConfig.cs`:

    config.Routes.MapHttpRoute(
        name: "DefaultApi",
        routeTemplate: "api/{controller}/{id}/{format}",
        defaults: new { id = RouteParameter.Optional, format = RouteParameter.Optional }
    );

To see the `JsonpMediaTypeFormatter` in action, just clone this project, run the `WebContribContrib.Formatting.Jsonp.SampleWebHost` project web application, and then start the `WebApiContrib.Formatting.Jsonp.SampleJQueryClient` web application and hit the "Get JSONP" button.

