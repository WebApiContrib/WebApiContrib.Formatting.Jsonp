using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace WebApiContrib.Formatting.Jsonp
{
    internal class JsonpQueryStringMapping : QueryStringMapping
    {
        public JsonpQueryStringMapping(string queryStringParameterName, MediaTypeHeaderValue mediaType)
            : base(queryStringParameterName, "*", mediaType)
        {
        }

        public JsonpQueryStringMapping(string queryStringParameterName, string mediaType)
            : base(queryStringParameterName, "*", mediaType)
        {
        }

        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            var queryString = request.RequestUri.ParseQueryString();
            return queryString.Keys.Cast<string>().Any(p => p == QueryStringParameterName) ? 1.0 : 0.0;
        }
    }
}
