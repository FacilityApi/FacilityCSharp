using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.ServerTesting
{
	public sealed class ClientTestingHttpHandler : HttpMessageHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string testName = request.Headers.TryGetValues("FacilityTest", out var values) ? string.Join(",", values) : null;
			if (testName == null)
			{
				return Fail("missing FacilityTest header");
			}
			else if (testName == "GetApiInfo")
			{
				var response = new Dictionary<string, object>
				{
					["service"] = "TestServerApi",
					["version"] = "0.1.0",
				};

				if (request.Method != HttpMethod.Post || request.RequestUri.AbsoluteUri != "http://example.com/finishTest")
				{
					var httpMethod = HttpMethod.Get;
					if (request.Method != httpMethod)
						return Fail($"unexpected HTTP method {request.Method}; should be {httpMethod}");

					var url = "http://example.com/";
					if (request.RequestUri.AbsoluteUri != url)
						return Fail($"unexpected URL {request.RequestUri.AbsoluteUri}; should be {url}");

					return Json(HttpStatusCode.OK, response);
				}
				else
				{
					string testJson = await request.Content.ReadAsStringAsync();

					if (!TestUtility.TryParseJson(testJson, out var jsonValue, out var badJsonMessage))
						return Fail($"test response has invalid JSON: {badJsonMessage}");

					if (!(jsonValue is IReadOnlyList<KeyValuePair<string, object>> jsonObject))
						return Fail("test response must be a JSON object");

					if (jsonObject.Count != 1)
						return Fail("test response must have exactly one property");

					if (jsonObject[0].Key == "error")
					{
						if (!(jsonObject[0].Value is IReadOnlyList<KeyValuePair<string, object>> errorObject))
							return Fail("test response error must be an object");

						string errorCode = errorObject.Where(x => x.Key == "code").Select(x => x.Value as string).FirstOrDefault();
						if (errorCode == null)
							return Fail("test response error code is missing");

						string errorMessage = errorObject.Where(x => x.Key == "message").Select(x => x.Value as string).FirstOrDefault();
						if (errorMessage == null)
							return Fail("test response error code is missing");

						if (errorCode == c_testFailCode)
							return Fail(errorMessage);

						return Fail($"unexpected error {errorCode}: {errorMessage}");
					}
					else if (jsonObject[0].Key == "value")
					{
						if (!TestUtility.IsJsonEquivalent(jsonObject[0].Value, response, out var message))
							return Fail($"test response has unexpected JSON: {message}");

						return Json(HttpStatusCode.OK,
							new Dictionary<string, object>
							{
								["status"] = "pass",
							});
					}
					else
					{
						return Fail("test response property must be 'value' or 'error'");
					}
				}
			}
			else if (testName == "CreateWidget")
			{
				var headers = new Dictionary<string, string>
				{
					["Location"] = "/widgets/1337",
					["ETag"] = "\"initial\"",
				};

				var body = new Dictionary<string, object>
				{
					["id"] = 1337,
					["name"] = "shiny",
				};

				if (request.Method != HttpMethod.Post || request.RequestUri.AbsoluteUri != "http://example.com/finishTest")
				{
					var httpMethod = HttpMethod.Post;
					if (request.Method != httpMethod)
						return Fail($"unexpected HTTP method {request.Method}; should be {httpMethod}");

					var url = "http://example.com/widgets";
					if (request.RequestUri.AbsoluteUri != url)
						return Fail($"unexpected URL {request.RequestUri.AbsoluteUri}; should be {url}");

					return Json(HttpStatusCode.Created, headers, body);
				}
				else
				{
					string testJson = await request.Content.ReadAsStringAsync();

					if (!TestUtility.TryParseJson(testJson, out var jsonValue, out var badJsonMessage))
						return Fail($"test response has invalid JSON: {badJsonMessage}");

					if (!(jsonValue is IReadOnlyList<KeyValuePair<string, object>> jsonObject))
						return Fail("test response must be a JSON object");

					if (jsonObject.Count != 1)
						return Fail("test response must have exactly one property");

					if (jsonObject[0].Key == "error")
					{
						if (!(jsonObject[0].Value is IReadOnlyList<KeyValuePair<string, object>> errorObject))
							return Fail("test response error must be an object");

						string errorCode = errorObject.Where(x => x.Key == "code").Select(x => x.Value as string).FirstOrDefault();
						if (errorCode == null)
							return Fail("test response error code is missing");

						string errorMessage = errorObject.Where(x => x.Key == "message").Select(x => x.Value as string).FirstOrDefault();
						if (errorMessage == null)
							return Fail("test response error code is missing");

						if (errorCode == c_testFailCode)
							return Fail(errorMessage);

						return Fail($"unexpected error {errorCode}: {errorMessage}");
					}
					else if (jsonObject[0].Key == "value")
					{
						if (!TestUtility.IsJsonEquivalent(jsonObject[0].Value,
							new Dictionary<string, object>
							{
								["widget"] = body,
								["url"] = "/widgets/1337",
								["eTag"] = "\"initial\"",
							}, out var message))
						{
							return Fail($"test response has unexpected JSON: {message}");
						}
					}
					else
					{
						return Fail("test response property must be 'value' or 'error'");
					}

					return Json(HttpStatusCode.OK,
						new Dictionary<string, object>
						{
							["status"] = "pass",
						});
				}
			}
			else
			{
				return Fail($"unknown test '{testName}'");
			}
		}

		private static HttpResponseMessage Fail(string message)
		{
			var error = new Dictionary<string, object>
			{
				["code"] = c_testFailCode,
				["message"] = message,
			};
			return Json(HttpStatusCode.BadRequest, error);
		}

		private static HttpResponseMessage Json(HttpStatusCode statusCode, object content) => Json(statusCode, null, content);

		private static HttpResponseMessage Json(HttpStatusCode statusCode, IEnumerable<KeyValuePair<string, string>> headers, object content)
		{
			var response = new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(TestUtility.RenderJson(content), Encoding.UTF8, "application/json")
			};

			if (headers != null)
			{
				foreach (var header in headers)
				{
					if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
						throw new InvalidOperationException($"Failed to add header {header.Key}: {header.Value}");
				}
			}

			return response;
		}

		private const string c_testFailCode = "TestFail";
	}
}
