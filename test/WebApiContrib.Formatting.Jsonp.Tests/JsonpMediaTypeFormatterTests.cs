using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
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

        static JsonpMediaTypeFormatter CreateFormatter()
        {
            var jsonFormatter = new JsonMediaTypeFormatter();
            return new JsonpMediaTypeFormatter(jsonFormatter);
        }
    }
}
