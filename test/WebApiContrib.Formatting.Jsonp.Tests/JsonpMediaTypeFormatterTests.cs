using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace WebApiContrib.Formatting.Jsonp.Tests
{
    public class JsonpMediaTypeFormatterTests
    {
        [Test]
        public void CanReadType_AlwaysReturnsFalse()
        {
            var formatter = CreateFormatter();
            Assert.False(formatter.CanReadType(typeof(Object)));
            Assert.False(formatter.CanReadType(typeof(string)));
            Assert.False(formatter.CanReadType(typeof(JToken)));
        }

        [TestCase("POST")]
        [TestCase("PUT")]
        [TestCase("DELETE")]
        [TestCase("HEAD")]
        [TestCase("OPTIONS")]
        [TestCase("TRACE")]
        public void IsJsonpRequest_ReturnsFalseForNonGetRequest(string httpMethod)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://example.org/");
            string callback;
            Assert.IsFalse(JsonpMediaTypeFormatter.IsJsonpRequest(request, "callback", out callback));
        }

        [Test]
        public void IsJsonpRequest_ReturnsFalseForGetRequestWithNoCallback()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.org/");
            string callback;
            Assert.IsFalse(JsonpMediaTypeFormatter.IsJsonpRequest(request, "callback", out callback));
        }

        [Test]
        public void IsJsonpRequest_ReturnsTrueForGetRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.org/?callback=?");
            string callback;
            Assert.IsTrue(JsonpMediaTypeFormatter.IsJsonpRequest(request, "callback", out callback));
        }

        // Ensure JSON works
        [TestCase("/api/value/1", "", "application/json", "\"value 1\"")]
        [TestCase("/api/value/1", "application/json", "application/json", "\"value 1\"")]
        [TestCase("/api/value/1?callback=?", "", "text/javascript", "?(\"value 1\");")]
        [TestCase("/api/value/1?callback=?", "application/json", "text/javascript", "?(\"value 1\");")]
        // text/javascript
        [TestCase("/api/value/1?callback=?", "text/javascript", "text/javascript", "?(\"value 1\");")]
        [TestCase("/api/value/1", "text/javascript", "text/javascript", "A callback parameter was not provided in the request URI.")]
        // application/javascript
        [TestCase("/api/value/1?callback=?", "application/javascript", "text/javascript", "?(\"value 1\");")]
        [TestCase("/api/value/1", "application/javascript", "text/javascript", "A callback parameter was not provided in the request URI.")]
        // application/json-p
        [TestCase("/api/value/1?callback=?", "application/json-p", "text/javascript", "?(\"value 1\");")]
        [TestCase("/api/value/1", "application/json-p", "text/javascript", "A callback parameter was not provided in the request URI.")]
        public async Task WriteToStreamAsync_TestExpectations(string requestUri, string acceptMediaType, string expectedMediaType, string expectedValue)
        {
            var config = new HttpConfiguration();
            config.Formatters.Add(CreateFormatter(config.Formatters.JsonFormatter));
            config.Routes.MapHttpRoute("Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                client.BaseAddress = new Uri("http://test.org/");

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                if (!string.IsNullOrEmpty(acceptMediaType))
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptMediaType));

                try
                {
                    var response = await client.SendAsync(request);
                    var content = response.Content;
                    Assert.AreEqual(expectedMediaType, content.Headers.ContentType.MediaType);

                    var text = await content.ReadAsStringAsync();
                    Assert.AreEqual(expectedValue, text);
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual(expectedValue, ex.Message);
                }
            }
        }

        static JsonpMediaTypeFormatter CreateFormatter(JsonMediaTypeFormatter formatter = null)
        {
            var jsonFormatter = formatter ?? new JsonMediaTypeFormatter();
            return new JsonpMediaTypeFormatter(jsonFormatter);
        }
    }
}
