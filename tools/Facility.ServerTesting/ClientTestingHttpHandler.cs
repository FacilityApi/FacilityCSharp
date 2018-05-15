using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Faithlife.Parsing;
using Faithlife.Parsing.Json;

namespace Facility.ServerTesting
{
	public sealed class ClientTestingHttpHandler : HttpMessageHandler
	{
		private static HttpResponseMessage Text(HttpStatusCode statusCode, string content)
		{
			return new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(content, Encoding.UTF8, "text/plain"),
			};
		}

		private static HttpResponseMessage Json(HttpStatusCode statusCode, object content)
		{
			return new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(TestUtility.RenderJson(content), Encoding.UTF8, "application/json"),
			};
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string testName = request.Headers.TryGetValues("FacilityTest", out var values) ? string.Join(",", values) : null;
			if (testName == null)
			{
				return Text(HttpStatusCode.NotImplemented, "missing FacilityTest header");
			}
			else if (testName == "GetApiInfo")
			{
				var response = new Dictionary<string, object>
				{
					["service"] = "TestServerApi",
					["version"] = "0.1.0",
				};

				if (request.Method == HttpMethod.Get && request.RequestUri.AbsoluteUri == "http://example.com/")
				{
					return Json(HttpStatusCode.OK, response);
				}
				else if (request.Method == HttpMethod.Post && request.RequestUri.AbsoluteUri == "http://example.com/finishTest")
				{
					string testJson = await request.Content.ReadAsStringAsync();

					var parseResult = JsonParsers.JsonObject.TryParse(testJson);
					if (!parseResult.Success)
						throw new TestFailedException($"response content invalid JSON: {parseResult.ToMessage()}");
					var parsedJson = parseResult.Value;
					var success = parsedJson.Where(x => x.Key == "value")
						.Select(x => (IEnumerable<KeyValuePair<string, object>>) x.Value)
						.Single();

					if (!TestUtility.IsJsonEquivalent(success, response, out var message))
						throw new TestFailedException(message);

					return new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent(TestUtility.RenderJson(new Dictionary<string, object>
						{
							["status"] = "pass",
						}), Encoding.UTF8, "application/json"),
					};
				}
				else
				{
					return Text(HttpStatusCode.NotImplemented,
						$"unexpected HTTP method and/or URL for test {testName}: {request.Method} {request.RequestUri.AbsoluteUri}");
				}
			}
			else
			{
				return Text(HttpStatusCode.NotImplemented, $"unexpected test {testName}");
			}
		}
	}
}
