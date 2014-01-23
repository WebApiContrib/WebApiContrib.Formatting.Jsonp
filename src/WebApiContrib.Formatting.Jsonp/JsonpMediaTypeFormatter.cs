using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApiContrib.Formatting.Jsonp
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle JSON-P.
    /// </summary>
    public class JsonpMediaTypeFormatter : MediaTypeFormatter
    {
        private static readonly MediaTypeHeaderValue _applicationJavaScript = new MediaTypeHeaderValue("application/javascript");
        private static readonly MediaTypeHeaderValue _applicationJsonp = new MediaTypeHeaderValue("application/json-p");
        private static readonly MediaTypeHeaderValue _textJavaScript = new MediaTypeHeaderValue("text/javascript");
        private readonly HttpRequestMessage _request;
        private readonly MediaTypeFormatter _jsonMediaTypeFormatter;
        private readonly string _callbackQueryParameter;
        private readonly string _callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonpMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="jsonMediaTypeFormatter">The <see cref="JsonMediaTypeFormatter"/> to use internally for JSON serialization.</param>
        /// <param name="callbackQueryParameter">The query parameter containing the callback.</param>
        public JsonpMediaTypeFormatter(MediaTypeFormatter jsonMediaTypeFormatter, string callbackQueryParameter = null)
        {
            if (jsonMediaTypeFormatter == null)
            {
                throw new ArgumentNullException("jsonMediaTypeFormatter");
            }

            _jsonMediaTypeFormatter = jsonMediaTypeFormatter;
            _callbackQueryParameter = callbackQueryParameter ?? "callback";

            SupportedMediaTypes.Add(_textJavaScript);
            SupportedMediaTypes.Add(_applicationJavaScript);
            SupportedMediaTypes.Add(_applicationJsonp);
            foreach (var encoding in _jsonMediaTypeFormatter.SupportedEncodings)
            {
                SupportedEncodings.Add(encoding);
            }
                
            MediaTypeMappings.Add(new JsonpQueryStringMapping(_callbackQueryParameter, _textJavaScript));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonpMediaTypeFormatter"/> class.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> for an instance formatter.</param>
        /// <param name="callback">The value of the callback query parameter.</param>
        /// <param name="jsonMediaTypeFormatter">The <see cref="JsonMediaTypeFormatter"/> to use internally for JSON serialization.</param>
        /// <param name="callbackQueryParameter">The query parameter containing the callback.</param>
        private JsonpMediaTypeFormatter(HttpRequestMessage request, string callback, MediaTypeFormatter jsonMediaTypeFormatter, string callbackQueryParameter)
            : this(jsonMediaTypeFormatter, callbackQueryParameter)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            _request = request;
            _callback = callback;
        }

        /// <summary>
        /// Returns a specialized instance of the <see cref="JsonpMediaTypeFormatter"/> that can handle formatting a response for the given
        /// parameters. This method is called after a formatter has been selected through content negotiation.
        /// </summary>
        /// <param name="type">The type being serialized.</param>
        /// <param name="request">The request.</param>
        /// <param name="mediaType">The media type chosen for the serialization. Can be <c>null</c>.</param>
        /// <returns>An instance that can format a response to the given <paramref name="request"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> parameter may not be null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="request"/> parameter may not be null.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="request"/> is not a valid JSON-P request.</exception>
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string callback;
            if (IsJsonpRequest(request, _callbackQueryParameter, out callback))
            {
                return new JsonpMediaTypeFormatter(request, callback, _jsonMediaTypeFormatter, _callbackQueryParameter);
            }

            throw new InvalidOperationException(Properties.Resources.NoCallback);
        }

        /// <summary>
        /// Determines whether this <see cref="JsonpMediaTypeFormatter"/> can read objects
        /// of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object that will be read.</param>
        /// <returns><c>true</c> if objects of this <paramref name="type"/> can be read, otherwise <c>false</c>.</returns>
        public override bool CanReadType(Type type)
        {
            return false;
        }

        /// <summary>
        /// Determines whether this <see cref="JsonpMediaTypeFormatter"/> can write objects
        /// of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object that will be written.</param>
        /// <returns><c>true</c> if objects of this <paramref name="type"/> can be written, otherwise <c>false</c>.</returns>
        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return _jsonMediaTypeFormatter.CanWriteType(type);
        }

        /// <summary>
        /// Called during serialization to write an object of the specified <paramref name="type"/>
        /// to the specified <paramref name="writeStream"/>.
        /// </summary>
        /// <param name="type">The type of object to write.</param>
        /// <param name="value">The object to write.</param>
        /// <param name="writeStream">The <see cref="Stream"/> to which to write.</param>
        /// <param name="content">The <see cref="HttpContent"/> for the content being written.</param>
        /// <param name="transportContext">The <see cref="TransportContext"/>.</param>
        /// <returns>A <see cref="Task"/> that will write the value to the stream.</returns>
        public override async Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var encoding = SelectCharacterEncoding(content == null ? null : content.Headers);
            using (var writer = new StreamWriter(stream, encoding, bufferSize: 4096, leaveOpen: true))
            {
                writer.Write(_callback + "(");
                writer.Flush();
                await _jsonMediaTypeFormatter.WriteToStreamAsync(type, value, stream, content, transportContext);
                writer.Write(");");
                writer.Flush();
            }
        }

        internal static bool IsJsonpRequest(HttpRequestMessage request, string callbackQueryParameter, out string callback)
        {
            callback = null;

            if (request == null || request.Method != HttpMethod.Get)
            {
                return false;
            }

            callback = request.GetQueryNameValuePairs()
                .Where(kvp => kvp.Key.Equals(callbackQueryParameter, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Value)
                .FirstOrDefault();

            return !string.IsNullOrEmpty(callback);
        }
    }
}
