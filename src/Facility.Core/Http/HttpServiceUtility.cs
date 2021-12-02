using System.Collections;
using System.Net.Http.Headers;

namespace Facility.Core.Http;

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

	internal static IReadOnlyDictionary<string, string> CreateDictionaryFromHeaders(HttpHeaders headers, HttpHeaders? moreHeaders) =>
		new DictionaryFromHeaders(moreHeaders is null ? new[] { headers } : new[] { headers, moreHeaders });

	internal static ServiceResult TryAddNonContentHeaders(HttpHeaders httpHeaders, IEnumerable<KeyValuePair<string, string?>>? headers)
	{
		if (headers != null)
		{
			foreach (var header in headers)
			{
				try
				{
					if (!s_supportedContentHeaders.Contains(header.Key) && header.Value != null)
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

	internal static string? GetContentType(this IEnumerable<KeyValuePair<string, string?>>? headers) =>
		headers?.FirstOrDefault(x => x.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)).Value;

	internal static int IndexOfOrdinal(this string value, char ch)
	{
#if NETSTANDARD2_0
			return value.IndexOf(ch);
#else
		return value.IndexOf(ch, StringComparison.Ordinal);
#endif
	}

	internal static string ReplaceOrdinal(this string value, string oldValue, string newValue)
	{
#if NETSTANDARD2_0
			return value.Replace(oldValue, newValue);
#else
		return value.Replace(oldValue, newValue, StringComparison.Ordinal);
#endif
	}

	internal static bool UsesBytesSerializer(Type objectType) => objectType == typeof(byte[]);

	internal static bool UsesTextSerializer(Type objectType) => objectType == typeof(string);

	private sealed class DictionaryFromHeaders : IReadOnlyDictionary<string, string>
	{
		public DictionaryFromHeaders(IReadOnlyList<HttpHeaders> httpHeaders)
		{
			m_httpHeaders = httpHeaders;
		}

		public int Count => m_httpHeaders.Sum(x => x.Count());

		public IEnumerable<string> Keys => this.Select(x => x.Key);

		public IEnumerable<string> Values => this.Select(x => x.Value);

		public bool ContainsKey(string key) => m_httpHeaders.Any(x => x.Contains(key));

		public bool TryGetValue(string key, out string value)
		{
			foreach (var httpHeaders in m_httpHeaders)
			{
				if (httpHeaders.TryGetValues(key, out var values))
				{
					value = JoinHeaderValues(values);
					return true;
				}
			}

			value = null!;
			return false;
		}

		public string this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() =>
			m_httpHeaders.SelectMany(x => x).Select(x => new KeyValuePair<string, string>(x.Key, JoinHeaderValues(x.Value))).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private static string JoinHeaderValues(IEnumerable<string> values) => string.Join(", ", values);

		private readonly IReadOnlyList<HttpHeaders> m_httpHeaders;
	}

	private static readonly IReadOnlyCollection<string> s_supportedContentHeaders =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Content-Type" };
}
