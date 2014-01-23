WebApiContrib.Formatting.Jsonp
==============================

`WebApiContrib.Formatting.Jsonp` provides a [JSONP](https://en.wikipedia.org/wiki/JSONP) [MediaTypeFormatter](http://msdn.microsoft.com/en-us/library/system.net.http.formatting.mediatypeformatter.aspx) implementation for [ASP.NET Web API](http://www.asp.net/web-api).

In order to add it to your Web API solution, run `Install-Package WebApiContrib.Formatting.Jsonp` from your NuGet Package Manager console in Visual Studio.

To use the `JsonpMediaTypeFormatter`, add the following code to your configuration in Global.asax.cs:

`GlobalConfiguration.Configuration.AddJsonpFormatter();`

You can specify a `MediaTypeFormatter` and callback parameter name as optional parameters. By default, the `JsonpMediaTypeFormatter` will use the `config.Formatters.JsonFormatter` and `callback` as default values.

You should specify `text/javascript` as the media type to accept. If you leave this out, the client may receive `application/json`. The `JsonpMediaTypeFormatter` will match the specified callback parameter name from the request URI, e.g. `?callback=?`.

If you are using traditional routing, update your Default ASP.NET Web API route in `/App_Start/WebApiConfig.cs`:

    config.Routes.MapHttpRoute(
        name: "DefaultApi",
        routeTemplate: "api/{controller}/{id}",
        defaults: new { id = RouteParameter.Optional }
    );

To see the `JsonpMediaTypeFormatter` in action, just clone this project, run the `WebContribContrib.Formatting.Jsonp.SampleWebHost` project web application, and then start the `WebApiContrib.Formatting.Jsonp.SampleJQueryClient` web application and hit the "Get JSONP" button.

