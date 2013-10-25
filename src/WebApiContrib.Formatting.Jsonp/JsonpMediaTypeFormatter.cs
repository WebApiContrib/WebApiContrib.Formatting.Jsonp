using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApiContrib.Formatting.Jsonp
{
	/// <summary>
	/// <see cref="MediaTypeFormatter"/> class to handle JSON-P.
	/// </summary>
	public class JsonpMediaTypeFormatter : MediaTypeFormatter
	{
		private readonly HttpRequestMessage _request;
		private readonly MediaTypeFormatter _jsonMediaTypeFormatter;
		private readonly string _callbackQueryParameter;
		private readonly string _callback;
		private readonly bool _queryStringMapping;
		private readonly bool _uriPathMapping;

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonpMediaTypeFormatter"/> class.
		/// </summary>
		/// <param name="jsonMediaTypeFormatter">The <see cref="JsonMediaTypeFormatter"/> to use internally for JSON serialization.</param>
		/// <param name="callbackQueryParameter">The query parameter containing the callback.</param>
		public JsonpMediaTypeFormatter(MediaTypeFormatter jsonMediaTypeFormatter, bool queryStringMapping = true, bool uriPathMapping = true, string callbackQueryParameter = "callback")
		{
			if (jsonMediaTypeFormatter == null)
			{
				throw new ArgumentNullException("jsonMediaTypeFormatter");
			}

			if (callbackQueryParameter == null)
			{
				throw new ArgumentNullException("callbackQueryParameter");
			}

			_jsonMediaTypeFormatter = jsonMediaTypeFormatter;
			_callbackQueryParameter = callbackQueryParameter;
			_queryStringMapping = queryStringMapping;
			_uriPathMapping = uriPathMapping;

			SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/javascript"));
			foreach (var encoding in _jsonMediaTypeFormatter.SupportedEncodings)
				SupportedEncodings.Add(encoding);
				
			// OPTIONAL DETECTION OF QUERYSTRING FORMAT PARAMETER (format=jsonp)
			if (_queryStringMapping) { MediaTypeMappings.Add(new QueryStringMapping("format", "jsonp", new MediaTypeHeaderValue("text/javascript"))); }
			
			// OPTIONAL DETECTION OF URI PATH EXTENSION /jsonp/
			if (_uriPathMapping) { MediaTypeMappings.Add(new UriPathExtensionMapping("jsonp", "application/json")); }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonpMediaTypeFormatter"/> class.
		/// </summary>
		/// <param name="request">The <see cref="HttpRequestMessage"/> for an instance formatter.</param>
		/// <param name="callback">The value of the callback query parameter.</param>
		/// <param name="jsonMediaTypeFormatter">The <see cref="JsonMediaTypeFormatter"/> to use internally for JSON serialization.</param>
		/// <param name="callbackQueryParameter">The query parameter containing the callback.</param>
		private JsonpMediaTypeFormatter(HttpRequestMessage request, string callback, MediaTypeFormatter jsonMediaTypeFormatter, bool queryStringMapping, bool uriPathMapping, string callbackQueryParameter)
			: this(jsonMediaTypeFormatter, true, true, callbackQueryParameter)
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
				return new JsonpMediaTypeFormatter(request, callback, _jsonMediaTypeFormatter, _queryStringMapping, _uriPathMapping, _callbackQueryParameter);
			}

			// Not JSONP? Let the JSON formatter handle it
			return _jsonMediaTypeFormatter.GetPerRequestFormatterInstance(type, request, mediaType);
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
		public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
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
			// TODO: .NET 4.5's API allows StreamWriter to leave the underlying stream open. This would allow us to close the writer when finished.
			var writer = new StreamWriter(stream, encoding);
			writer.Write(_callback + "(");
			writer.Flush();

			// TODO: Use async/await in .NET 4.5
			return _jsonMediaTypeFormatter.WriteToStreamAsync(type, value, stream, content, transportContext)
				.Then(() =>
				{
					writer.Write(");");
					writer.Flush();
					// TODO: Once on .NET 4.5 and the writer leaves the underlying stream open, dispose the writer.
				});
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
