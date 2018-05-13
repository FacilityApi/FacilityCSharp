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

		protected async Task VerifyResponse(HttpMethod httpMethod, string path, object body, HttpStatusCode statusCode,
			bool anyContent = false, Dictionary<string, object> json = null, Dictionary<string, string> headers = null)
		{
			var request = new HttpRequestMessage(httpMethod, $"{m_root}{path}");

			if (body != null)
			{
				if (body is IEnumerable<KeyValuePair<string, object>> bodyJson)
				{
					request.Content = new StringContent(RenderJson(bodyJson), Encoding.UTF8, "application/json");
				}
				else
				{
					throw new ArgumentException($"Unrecognized body type: {body.GetType().FullName}");
				}
			}

			var response = await m_http.SendAsync(request);

			var actualStatusCode = response.StatusCode;
			if (actualStatusCode != statusCode)
				throw new TestFailedException($"status code {(int) actualStatusCode}; expected {(int) statusCode}");

			if (!anyContent)
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

			if (headers != null)
			{
				foreach (var header in headers)
				{
					string actualHeader = string.Join(",", response.Headers.GetValues(header.Key));
					if (actualHeader != header.Value)
						throw new TestFailedException($"response header '{header.Key}' is '{actualHeader}'; expected '{header.Value}'");
				}
			}
		}

		protected static Func<object, bool> Matches(Func<object, bool> predicate) => predicate;

		private static string RenderJson(object json)
		{
			if (json is IEnumerable<KeyValuePair<string, object>> jsonObject)
			{
				return "{" + string.Join(",", jsonObject.Select(x => RenderJson(x))) + "}";
			}
			else if (json is KeyValuePair<string, object> jsonProperty)
			{
				return RenderJson(jsonProperty.Key) + ":" + RenderJson(jsonProperty.Value);
			}
			else if (json is string jsonString)
			{
				return "\"" + jsonString + "\"";
			}
			else
			{
				return json?.ToString() ?? "null";
			}
		}

		private static void VerifyJsonEquivalent(object actual, object expected, string path = "")
		{
			if (actual is int actualInt32)
				actual = (long) actualInt32;
			if (expected is int expectedInt32)
				expected = (long) expectedInt32;

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
			else if (expected is Func<object, bool> predicate)
			{
				if (!predicate(actual))
					throw new TestFailedException(FormattableString.Invariant($"{renderPath("")} was {getTypeName(actual)} '{actual}'"));
			}
			else if (!Equals(actual, expected))
			{
				throw new TestFailedException(FormattableString.Invariant($"{renderPath("")} was {getTypeName(actual)} '{actual}'; expected {getTypeName(expected)} {expected}"));
			}

			string getTypeName(object o)
			{
				if (o is IEnumerable<KeyValuePair<string, object>>)
					return "object";
				if (o is IReadOnlyList<object>)
					return "array";
				if (o is int || o is long || o is double)
					return "number";
				if (o is string)
					return "string";
				if (o is bool)
					return "boolean";
				if (o == null)
					return "null";
				throw new InvalidOperationException($"Unexpected type {o.GetType().FullName}");
			}

			string combinePath(string a, string b) => a.Length == 0 ? b : $"{a}.{b}";

			string renderPath(string t) => (t.Length == 0 ? "response" : $"response {t}") + (path.Length == 0 ? "" : $" at '{path}'");
		}

		private readonly HttpClient m_http;
		private readonly string m_root;
	}
}
