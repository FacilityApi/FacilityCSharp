using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
		/// Creates an HTTP client with default settings.
		/// </summary>
		public static HttpClient CreateHttpClient()
		{
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.ExpectContinue = false;
			return httpClient;
		}

		/// <summary>
		/// Creates HTTP content for the specified DTO.
		/// </summary>
		public static HttpContent CreateHttpContent(ServiceDto content, string mediaType)
		{
			if (mediaType == JsonMediaType)
				return new DelegateHttpContent(mediaType, stream => ServiceJsonUtility.ToJsonStream(content, stream));
			else
				throw new InvalidOperationException("Unsupported media type: " + mediaType);
		}

		/// <summary>
		/// Reads a DTO from the specified HTTP content.
		/// </summary>
		public static async Task<ServiceResult<T>> ReadHttpContentAsync<T>(HttpContent content, string errorCode)
			where T : ServiceDto
		{
			return (await ReadHttpContentAsync(typeof(T), content, errorCode).ConfigureAwait(false)).Map(x => (T) x);
		}

		/// <summary>
		/// Reads a DTO from the specified HTTP content.
		/// </summary>
		public static async Task<ServiceResult<ServiceDto>> ReadHttpContentAsync(Type dtoType, HttpContent content, string errorCode)
		{
			if (content == null || content.Headers.ContentLength == 0)
				return ServiceResult.Success<ServiceDto>(null);

			var contentType = content.Headers.ContentType;
			if (contentType == null)
				return ServiceResult.Failure(HttpServiceErrors.CreateMissingContentType(errorCode));

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
					return ServiceResult.Failure(HttpServiceErrors.CreateInvalidContent(errorCode, exception.Message));
				}
			}
			else
			{
				return ServiceResult.Failure(HttpServiceErrors.CreateUnsupportedContentType(errorCode, mediaType));
			}
		}

		/// <summary>
		/// The JSON media type.
		/// </summary>
		public const string JsonMediaType = "application/json";

		internal static IReadOnlyDictionary<string, string> CreateDictionaryFromHeaders(HttpHeaders headers)
		{
			return new DictionaryFromHeaders(headers);
		}

		internal static ServiceResult TryAddHeaders(HttpHeaders httpHeaders, IEnumerable<KeyValuePair<string, string>> headers)
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
							HttpServiceErrors.CreateResponseHeaderInvalidFormat(header.Key) :
							HttpServiceErrors.CreateRequestHeaderInvalidFormat(header.Key));
					}
					catch (InvalidOperationException)
					{
						return ServiceResult.Failure(httpHeaders is HttpResponseHeaders ?
							HttpServiceErrors.CreateResponseHeaderNotSupported(header.Key) :
							HttpServiceErrors.CreateRequestHeaderNotSupported(header.Key));
					}
				}
			}

			return ServiceResult.Success();
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
	}
}
