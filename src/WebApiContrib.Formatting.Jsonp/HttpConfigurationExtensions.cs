using System.Net.Http.Formatting;
using System.Web.Http;

namespace WebApiContrib.Formatting.Jsonp
{
    public static class HttpConfigurationExtensions
    {
        public static void AddJsonpFormatter(this HttpConfiguration config, MediaTypeFormatter jsonFormatter = null, string callbackQueryParameter = null)
        {
            config.Formatters.Add(new JsonpMediaTypeFormatter(jsonFormatter ?? config.Formatters.JsonFormatter, callbackQueryParameter));
        }
    }
}
