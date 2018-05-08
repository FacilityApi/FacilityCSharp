using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Faithlife.Parsing;
using Faithlife.Parsing.Json;

namespace Facility.ServerTesting
{
	internal sealed class ServerTests
	{
		public ServerTests(HttpClient httpClient, string rootPath)
		{
			m_http = httpClient;
			m_root = rootPath;
		}

		public async Task ApiInfo()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, m_root);

			var response = await m_http.SendAsync(request);

			var expectedStatusCode = HttpStatusCode.OK;
			var actualStatusCode = response.StatusCode;
			if (actualStatusCode != expectedStatusCode)
				throw new TestFailedException($"status code {(int) actualStatusCode}; expected {(int) expectedStatusCode}");

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

			VerifyJsonEquivalent(parseResult.Value,
				new Dictionary<string, object>
				{
					["service"] = "TestServerApi",
					["version"] = "0.1.0",
				});
		}

		public async Task DeleteApiInfo()
		{
			var request = new HttpRequestMessage(HttpMethod.Delete, m_root);

			var response = await m_http.SendAsync(request);

			var expectedStatusCode = HttpStatusCode.NotFound;
			var actualStatusCode = response.StatusCode;
			if (actualStatusCode != expectedStatusCode)
				throw new TestFailedException($"status code {(int) actualStatusCode}; expected {(int) expectedStatusCode}");
		}

		private static void VerifyJsonEquivalent(object actual, object expected, string path = "")
		{
			if (expected is IEnumerable<KeyValuePair<string, object>> expectedObject)
			{
				if (!(actual is IEnumerable<KeyValuePair<string, object>> actualObject))
					throw new TestFailedException($"{renderPath("")} was not an object");

				using (var actualIterator = actualObject.GetEnumerator())
				{
					foreach (var expectedPair in expectedObject.OrderBy(x => x.Key, StringComparer.Ordinal))
					{
						if (!actualIterator.MoveNext() || actualIterator.Current.Key != expectedPair.Key)
							throw new TestFailedException($"{renderPath("object")} missing property '{expectedPair.Key}'");
						VerifyJsonEquivalent(actualIterator.Current.Value, expectedPair.Value, combinePath(path, expectedPair.Key));
					}

					if (actualIterator.MoveNext())
						throw new TestFailedException($"{renderPath("object")} has extra property '{actualIterator.Current.Key}'");
				}
			}
			else if (!Equals(actual, expected))
			{
				string getTypeName(object o)
				{
					if (o is IEnumerable<KeyValuePair<string, object>>)
						return "object";
					if (o is IReadOnlyList<object>)
						return "array";
					if (o is long || o is double)
						return "number";
					if (o is bool)
						return "boolean";
					if (o == null)
						return "null";
					throw new InvalidOperationException($"Unexpected type {o.GetType().FullName}");
				}

				throw new TestFailedException(FormattableString.Invariant($"response was {getTypeName(actual)} '{actual}'; expected {getTypeName(expected)} {expected}"));
			}

			string combinePath(string a, string b) => a.Length == 0 ? b : $"{a}.{b}";

			string renderPath(string t) => (t.Length == 0 ? "response" : $"response {t}") + (path.Length == 0 ? "" : $"at '{path}'");
		}

		private readonly HttpClient m_http;
		private readonly string m_root;
	}
}
