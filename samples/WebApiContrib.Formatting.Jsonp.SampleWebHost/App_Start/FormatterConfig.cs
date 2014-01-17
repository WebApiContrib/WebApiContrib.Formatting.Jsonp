using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApiContrib.Formatting.Jsonp;

namespace WebContribContrib.Formatting.Jsonp.SampleWebHost.App_Start
{
    public class FormatterConfig
    {
        public static void RegisterFormatters(MediaTypeFormatterCollection formatters)
        {
            var jsonFormatter = formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var jsonpFormatter = new JsonpMediaTypeFormatter(jsonFormatter);
            formatters.Add(jsonpFormatter);
        }
    }
}
