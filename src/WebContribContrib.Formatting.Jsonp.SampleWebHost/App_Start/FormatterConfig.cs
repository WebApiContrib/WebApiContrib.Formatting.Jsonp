using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApiContrib.Formatting.Jsonp;

namespace WebContribContrib.Formatting.Jsonp.SampleWebHost.App_Start{
	public class FormatterConfig{
		public static void RegisterFormatters(MediaTypeFormatterCollection formatters){
			formatters.Remove(formatters.JsonFormatter);
			formatters.Insert(0, new JsonpMediaTypeFormatter {
				SerializerSettings = new JsonSerializerSettings {
					ContractResolver = new CamelCasePropertyNamesContractResolver()
				}
			});
		}
	}
}