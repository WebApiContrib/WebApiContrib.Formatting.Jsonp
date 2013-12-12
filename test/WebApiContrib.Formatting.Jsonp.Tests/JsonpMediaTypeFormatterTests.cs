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

        [Test]
        public async Task WriteToStreamAsync_WrapsResponseWithCallbackAndParens()
        {
            var config = new HttpConfiguration();
            config.Formatters.Insert(0, CreateFormatter(config.Formatters.JsonFormatter));
            config.Routes.MapHttpRoute("Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            using (var server = new HttpServer(config))
            using (var client = new HttpClient(server))
            {
                client.BaseAddress = new Uri("http://test.org/");

                var request = new HttpRequestMessage(HttpMethod.Get, "/api/value/1?callback=?");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));

                var response = await client.SendAsync(request);
                var content = response.Content;
                Assert.AreEqual("text/javascript", content.Headers.ContentType.MediaType);

                var text = await content.ReadAsStringAsync();
                Assert.AreEqual("?(\"value 1\");", text);
            }
        }

        static JsonpMediaTypeFormatter CreateFormatter(JsonMediaTypeFormatter formatter = null)
        {
            var jsonFormatter = formatter ?? new JsonMediaTypeFormatter();
            return new JsonpMediaTypeFormatter(jsonFormatter);
        }
    }
}
