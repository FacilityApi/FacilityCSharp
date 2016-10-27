using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Facility.Core.Http
{
	/// <summary>
	/// Utility methods for HTTP services.
	/// </summary>
	public static class HttpServiceUtility
	{
		/// <summary>
		/// Creates a request for the specified URI and method.
		/// </summary>
		public static HttpRequestMessage CreateHttpRequest(Uri baseUri, HttpMethod httpMethod, string relativeUriPattern, IEnumerable<KeyValuePair<string, string>> uriParameters = null)
		{
			return TryCreateHttpRequest(baseUri, httpMethod, relativeUriPattern, uriParameters).GetValueOrDefault();
		}

		/// <summary>
		/// Creates a request for the specified URI and method.
		/// </summary>
		public static ServiceResult<HttpRequestMessage> TryCreateHttpRequest(Uri baseUri, HttpMethod httpMethod, string relativeUriPattern, IEnumerable<KeyValuePair<string, string>> uriParameters = null, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
		{
			string uriText = baseUri.AbsoluteUri;

			if (!string.IsNullOrEmpty(relativeUriPattern))
				uriText = uriText.TrimEnd('/') + "/" + relativeUriPattern.TrimStart('/');

			Uri uri = uriParameters != null ? GetUriFromPattern(uriText, uriParameters) : new Uri(uriText);
			var requestMessage = new HttpRequestMessage(httpMethod, uri);

			var headersResult = TryAddHeaders(requestMessage.Headers, requestHeaders);
			if (headersResult.IsFailure)
				return headersResult.AsFailure();

			return ServiceResult.Success(requestMessage);
		}

		/// <summary>
		/// Attempts to set the HTTP headers.
		/// </summary>
		public static ServiceResult TryAddHeaders(HttpHeaders httpHeaders, IEnumerable<KeyValuePair<string, string>> headers)
		{
			if (headers != null)
			{
				foreach (var header in headers)
				{
					try
					{
						if (header.Value != null)
							httpHeaders.Add(header.Key, header.Value);
					}
					catch (FormatException)
					{
						return ServiceResult.Failure(httpHeaders is HttpResponseHeaders ?
							ServiceErrors.CreateResponseHeaderInvalidFormat(header.Key) :
							ServiceErrors.CreateRequestHeaderInvalidFormat(header.Key));
					}
					catch (InvalidOperationException)
					{
						return ServiceResult.Failure(httpHeaders is HttpResponseHeaders ?
							ServiceErrors.CreateResponseHeaderNotSupported(header.Key) :
							ServiceErrors.CreateRequestHeaderNotSupported(header.Key));
					}
				}
			}

			return ServiceResult.Success();
		}

		/// <summary>
		/// Creates HTTP content for the specified DTO.
		/// </summary>
		public static Task<HttpContent> CreateHttpContentAsync(ServiceDto content, string mediaType)
		{
			if (mediaType == JsonMediaType)
				return Task.FromResult((HttpContent) new DelegateHttpContent(mediaType, stream => ServiceJsonUtility.ToJsonStream(content, stream)));
			else
				throw new InvalidOperationException("Unsupported media type: " + mediaType);
		}

		/// <summary>
		/// Creates a DTO from the specified HTTP content; null if there is no content.
		/// </summary>
		public static Task<ServiceResult<T>> TryReadHttpRequestContentAsync<T>(HttpContent content) where T : ServiceDto
		{
			return TryReadHttpContentAsync<T>(content, ServiceErrors.InvalidRequest);
		}

		/// <summary>
		/// Creates a DTO from the specified HTTP content; null if there is no content.
		/// </summary>
		public static Task<ServiceResult<T>> TryReadHttpResponseContentAsync<T>(HttpContent content) where T : ServiceDto
		{
			return TryReadHttpContentAsync<T>(content, ServiceErrors.InvalidResponse);
		}

		/// <summary>
		/// Creates a DTO from the specified HTTP content; null if there is no content.
		/// </summary>
		public static async Task<ServiceResult<T>> TryReadHttpContentAsync<T>(HttpContent content, string errorCode) where T : ServiceDto
		{
			return (await TryReadHttpContentAsync(typeof(T), content, errorCode).ConfigureAwait(false)).Map(x => (T) x);
		}

		/// <summary>
		/// Creates a DTO from the specified HTTP content; null if there is no content.
		/// </summary>
		public static async Task<ServiceResult<ServiceDto>> TryReadHttpContentAsync(Type dtoType, HttpContent content, string errorCode)
		{
			if (content == null || content.Headers.ContentLength == 0)
				return ServiceResult.Success<ServiceDto>(null);

			var contentType = content.Headers.ContentType;
			if (contentType == null)
				return ServiceResult.Failure(HttpServiceErrors.CreateErrorMissingContentType(errorCode));

			string mediaType = contentType.MediaType;
			if (mediaType == JsonMediaType)
			{
				try
				{
					using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
					using (var textReader = new StreamReader(stream))
						return ServiceResult.Success((ServiceDto) ServiceJsonUtility.FromJsonTextReader(textReader, dtoType));
				}
				catch (JsonException exception)
				{
					return ServiceResult.Failure(HttpServiceErrors.CreateErrorBadContent(errorCode, exception.Message));
				}
			}
			else
			{
				return ServiceResult.Failure(HttpServiceErrors.CreateErrorUnsupportedContentType(errorCode, mediaType));
			}
		}

		/// <summary>
		/// Creates an error for the specified HTTP response.
		/// </summary>
		public static async Task<ServiceErrorDto> CreateErrorFromHttpResponseAsync(HttpResponseMessage response)
		{
			var result = await TryReadHttpResponseContentAsync<ServiceErrorDto>(response.Content).ConfigureAwait(false);

			if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value?.Code))
				return HttpServiceErrors.CreateErrorForStatusCode(response.StatusCode, response.ReasonPhrase);

			return result.Value;
		}

		/// <summary>
		/// Creates an HTTP response for the specified status code and DTO.
		/// </summary>
		public static async Task<HttpResponseMessage> CreateHttpResponseAsync<TContent>(HttpStatusCode statusCode, TContent content, string mediaType)
			where TContent : ServiceDto<TContent>
		{
			return new HttpResponseMessage(statusCode)
			{
				Content = await CreateHttpContentAsync(content, mediaType).ConfigureAwait(false)
			};
		}

		/// <summary>
		/// Creates an HTTP response for an error.
		/// </summary>
		public static Task<HttpResponseMessage> CreateHttpResponseFromErrorAsync(ServiceErrorDto error, string mediaType)
		{
			return CreateHttpResponseFromErrorAsync(error, mediaType, tryGetCustomHttpStatusCode: null);
		}

		/// <summary>
		/// Creates an HTTP response for an error.
		/// </summary>
		public static async Task<HttpResponseMessage> CreateHttpResponseFromErrorAsync(ServiceErrorDto error, string mediaType, Func<string, HttpStatusCode?> tryGetCustomHttpStatusCode)
		{
			HttpStatusCode statusCode = tryGetCustomHttpStatusCode?.Invoke(error.Code) ??
				HttpServiceErrors.TryGetHttpStatusCode(error.Code) ?? HttpStatusCode.InternalServerError;
			return await CreateHttpResponseAsync(statusCode, error, mediaType).ConfigureAwait(false);
		}

		/// <summary>
		/// Attempts to match the specified route, returning the path parameters if any, or null if the route doesn't match.
		/// </summary>
		public static IReadOnlyDictionary<string, string> TryMatchHttpRoute(Uri requestUri, string routePath)
		{
			string requestPath = requestUri.AbsolutePath.Trim('/');
			routePath = routePath.Trim('/');

			if (routePath.IndexOf('{') != -1)
			{
				var names = GetPathParameterNames(routePath);
				string regexPattern = Regex.Escape(routePath);
				foreach (string name in names)
					regexPattern = regexPattern.Replace("\\{" + name + "}", "(?'" + name + "'[^/]+)");
				regexPattern = "^(?:" + regexPattern + ")$";
				Match match = new Regex(regexPattern, RegexOptions.CultureInvariant).Match(requestPath);
				return match.Success ? names.ToDictionary(name => name, name => Uri.UnescapeDataString(match.Groups[name].ToString())) : null;
			}

			if (string.Equals(requestPath, routePath, StringComparison.OrdinalIgnoreCase))
				return s_emptyDictionary;

			return null;
		}

		/// <summary>
		/// Gets the names of any path parameters.
		/// </summary>
		public static IReadOnlyList<string> GetPathParameterNames(string routePath)
		{
			return s_regexPathParameterRegex.Matches(routePath).Cast<Match>().Select(x => x.Groups[1].ToString()).ToList();
		}

		/// <summary>
		/// Parses a query string.
		/// </summary>
		public static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseQueryString(string query)
		{
			if (query.Length != 0 && query[0] == '?')
				query = query.Substring(1);

			return query.Split('&')
				.Select(x => x.Split(new[] { '=' }, 2))
				.GroupBy(x => Uri.UnescapeDataString(x[0]), x => Uri.UnescapeDataString(x.Length == 1 ? "" : x[1]), StringComparer.OrdinalIgnoreCase)
				.ToDictionary(x => x.Key, x => (IReadOnlyList<string>) x.ToList());
		}

		/// <summary>
		/// Check if the media type is supported.
		/// </summary>
		public static bool IsSupportedMediaType(string mediaType)
		{
			return mediaType == JsonMediaType;
		}

		/// <summary>
		/// Converts HTTP headers into a simple string-to-string dictionary.
		/// </summary>
		public static IReadOnlyDictionary<string, string> CreateDictionaryFromHeaders(HttpHeaders headers)
		{
			return new DictionaryFromHeaders(headers);
		}

		/// <summary>
		/// The JSON media type.
		/// </summary>
		public const string JsonMediaType = "application/json";

		private static Uri GetUriFromPattern(string uriPattern, IEnumerable<KeyValuePair<string, string>> parameters)
		{
			bool hasQuery = uriPattern.IndexOf('?') != -1;

			foreach (KeyValuePair<string, string> parameter in parameters)
			{
				if (parameter.Key != null && parameter.Value != null)
				{
					string bracketedKey = "{" + parameter.Key + "}";
					int bracketedKeyIndex = uriPattern.IndexOf(bracketedKey, StringComparison.Ordinal);
					if (bracketedKeyIndex != -1)
					{
						uriPattern = uriPattern.Substring(0, bracketedKeyIndex) +
							Uri.EscapeDataString(parameter.Value) + uriPattern.Substring(bracketedKeyIndex + bracketedKey.Length);
					}
					else
					{
						uriPattern += (hasQuery ? "&" : "?") + Uri.EscapeDataString(parameter.Key) + "=" + Uri.EscapeDataString(parameter.Value);
						hasQuery = true;
					}
				}
			}

			return new Uri(uriPattern);
		}

		private sealed class DelegateHttpContent : HttpContent
		{
			public DelegateHttpContent(string mediaType, Action<Stream> writeToStream)
			{
				m_writeToStream = writeToStream;
				Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
			}

			protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
			{
				m_writeToStream(stream);
				return Task.FromResult<object>(null);
			}

			protected override bool TryComputeLength(out long length)
			{
				length = -1L;
				return false;
			}

			readonly Action<Stream> m_writeToStream;
		}

		private sealed class DictionaryFromHeaders : IReadOnlyDictionary<string, string>
		{
			public DictionaryFromHeaders(HttpHeaders httpHeaders)
			{
				m_httpHeaders = httpHeaders;
			}

			public int Count => m_httpHeaders.Count();

			public IEnumerable<string> Keys => this.Select(x => x.Key);

			public IEnumerable<string> Values => this.Select(x => x.Value);

			public bool ContainsKey(string key)
			{
				return m_httpHeaders.Contains(key);
			}

			public bool TryGetValue(string key, out string value)
			{
				IEnumerable<string> values;
				if (m_httpHeaders.TryGetValues(key, out values))
				{
					value = JoinHeaderValues(values);
					return true;
				}
				else
				{
					value = null;
					return false;
				}
			}

			public string this[string key]
			{
				get
				{
					string value;
					if (!TryGetValue(key, out value))
						throw new KeyNotFoundException();
					return value;
				}
			}

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			{
				return m_httpHeaders.Select(x => new KeyValuePair<string, string>(x.Key, JoinHeaderValues(x.Value))).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			private static string JoinHeaderValues(IEnumerable<string> values)
			{
				return string.Join(", ", values);
			}

			readonly HttpHeaders m_httpHeaders;
		}

		static readonly IReadOnlyDictionary<string, string> s_emptyDictionary = new Dictionary<string, string>();
		static readonly Regex s_regexPathParameterRegex = new Regex(@"\{([a-zA-Z][a-zA-Z0-9]*)\}", RegexOptions.CultureInvariant);
	}
}
