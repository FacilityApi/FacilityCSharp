using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

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
		/// The JSON media type.
		/// </summary>
		public const string JsonMediaType = "application/json";

		internal static IReadOnlyDictionary<string, string> CreateDictionaryFromHeaders(HttpHeaders headers) => new DictionaryFromHeaders(headers);

		internal static ServiceResult TryAddHeaders(HttpHeaders httpHeaders, IEnumerable<KeyValuePair<string, string?>>? headers)
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
						return ServiceResult.Failure(HttpServiceErrors.CreateHeaderInvalidFormat(header.Key));
					}
					catch (InvalidOperationException)
					{
						return ServiceResult.Failure(HttpServiceErrors.CreateHeaderNotSupported(header.Key));
					}
				}
			}

			return ServiceResult.Success();
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

			public bool ContainsKey(string key) => m_httpHeaders.Contains(key);

			public bool TryGetValue(string key, out string value)
			{
				if (m_httpHeaders.TryGetValues(key, out var values))
				{
					value = JoinHeaderValues(values);
					return true;
				}
				else
				{
					value = null!;
					return false;
				}
			}

			public string this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator() =>
				m_httpHeaders.Select(x => new KeyValuePair<string, string>(x.Key, JoinHeaderValues(x.Value))).GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			private static string JoinHeaderValues(IEnumerable<string> values) => string.Join(", ", values);

			readonly HttpHeaders m_httpHeaders;
		}
	}
}
