using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Faithlife.Parsing;
using Faithlife.Parsing.Json;

namespace Facility.ServerTesting
{
	internal abstract class ServerTestsBase
	{
		protected ServerTestsBase(HttpClient httpClient, string rootPath)
		{
			m_http = httpClient;
			m_root = rootPath;
		}

		protected async Task VerifyResponseAsync(HttpMethod httpMethod, string path,
			HttpStatusCode statusCode, bool anyContent = false, IReadOnlyDictionary<string, string> headers = null, Dictionary<string, object> json = null)
		{
			await VerifyResponseAsync(httpMethod, path, null, null, statusCode, anyContent, headers, json);
		}

		protected async Task VerifyResponseAsync(HttpMethod httpMethod, string path, object requestContent,
			HttpStatusCode statusCode, bool anyContent = false, IReadOnlyDictionary<string, string> headers = null, Dictionary<string, object> json = null)
		{
			await VerifyResponseAsync(httpMethod, path, null, requestContent, statusCode, anyContent, headers, json);
		}

		protected async Task VerifyResponseAsync(HttpMethod httpMethod, string path, IReadOnlyDictionary<string, string> requestHeaders, object requestContent,
			HttpStatusCode statusCode, bool anyContent = false, IReadOnlyDictionary<string, string> headers = null, Dictionary<string, object> json = null)
		{
			var request = new HttpRequestMessage(httpMethod, $"{m_root}{path}");

			if (requestHeaders != null)
			{
				foreach (var header in requestHeaders)
				{
					if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
						throw new ArgumentException($"Failed to add request header '{header.Key}' value '{header.Value}'.");
				}
			}

			if (requestContent != null)
			{
				if (requestContent is IEnumerable<KeyValuePair<string, object>> bodyJson)
					request.Content = new StringContent(TestUtility.RenderJson(bodyJson), Encoding.UTF8, "application/json");
				else
					throw new ArgumentException($"Unrecognized type of request content: {requestContent.GetType().FullName}");
			}

			var response = await m_http.SendAsync(request);

			var actualStatusCode = response.StatusCode;
			if (actualStatusCode != statusCode)
				throw new TestFailedException($"status code {(int) actualStatusCode}; expected {(int) statusCode}");

			if (!anyContent)
			{
				if (json != null)
				{
					if (response.Content == null)
						throw new TestFailedException("response content missing");

					string expectedContentType = "application/json";
					string actualContentType = response.Content.Headers.ContentType.ToString();
					if (actualContentType != expectedContentType)
						throw new TestFailedException($"response content type {actualContentType}; expected {expectedContentType}");

					string actualContent = await response.Content.ReadAsStringAsync();
					var parseResult = JsonParsers.JsonObject.TryParse(actualContent);
					if (!parseResult.Success)
						throw new TestFailedException($"response content invalid JSON: {parseResult.ToMessage()}");

					VerifyJsonEquivalent(parseResult.Value, json);
				}
				else
				{
					if (response.Content != null)
						throw new TestFailedException("unexpected response content");
				}
			}

			if (headers != null)
			{
				foreach (var header in headers)
				{
					if (!response.Headers.TryGetValues(header.Key, out var values))
						throw new TestFailedException($"response header '{header.Key}' is missing; expected '{header.Value}'");
					string actualHeader = string.Join(",", values);
					if (actualHeader != header.Value)
						throw new TestFailedException($"response header '{header.Key}' is '{actualHeader}'; expected '{header.Value}'");
				}
			}
		}

		protected static Func<object, bool> Matches(Func<object, bool> predicate) => predicate;

		private static void VerifyJsonEquivalent(object actual, object expected)
		{
			if (!TestUtility.IsJsonEquivalent(actual, expected, out var message))
				throw new TestFailedException(message);
		}

		private readonly HttpClient m_http;
		private readonly string m_root;
	}
}
