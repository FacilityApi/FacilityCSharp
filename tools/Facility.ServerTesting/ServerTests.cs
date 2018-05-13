using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Facility.ServerTesting
{
	internal sealed class ServerTests : ServerTestsBase
	{
		public ServerTests(HttpClient httpClient, string rootPath)
			: base(httpClient, rootPath)
		{
		}

		public async Task ApiInfo()
		{
			await VerifyResponse(HttpMethod.Get, "", null,
				HttpStatusCode.OK,
				json: new Dictionary<string, object>
				{
					["service"] = "TestServerApi",
					["version"] = "0.1.0",
				});
		}

		public async Task DeleteApiInfo()
		{
			await VerifyResponse(HttpMethod.Delete, "", null,
				HttpStatusCode.NotFound,
				anyContent: true);
		}

		public async Task CreateWidget()
		{
			await VerifyResponse(HttpMethod.Post, "/widgets",
				new Dictionary<string, object>
				{
					["name"] = "shiny",
				},
				HttpStatusCode.Created,
				json: new Dictionary<string, object>
				{
					["id"] = 1337,
					["name"] = "shiny",
				},
				headers: new Dictionary<string, string>
				{
					["Location"] = "http://example.com/widgets/1337",
					["ETag"] = "\"initial\"",
				});
		}

		public async Task CreateWidget_NoRequestBody()
		{
			await VerifyResponse(HttpMethod.Post, "/widgets", null,
				HttpStatusCode.BadRequest,
				json: new Dictionary<string, object>
				{
					["code"] = "InvalidRequest",
					["message"] = Matches(x => x is string s && s.Length != 0),
				});
		}

		public async Task CreateWidget_IdSpecified()
		{
			await VerifyResponse(HttpMethod.Post, "/widgets",
				new Dictionary<string, object>
				{
					["id"] = 1337,
					["name"] = "shiny",
				},
				HttpStatusCode.BadRequest,
				json: new Dictionary<string, object>
				{
					["code"] = "InvalidRequest",
					["message"] = Matches(x => x is string s && s.Length != 0),
				});
		}

		public async Task CreateWidget_NameMissing()
		{
			await VerifyResponse(HttpMethod.Post, "/widgets",
				new Dictionary<string, object>
				{
				},
				HttpStatusCode.BadRequest,
				json: new Dictionary<string, object>
				{
					["code"] = "InvalidRequest",
					["message"] = Matches(x => x is string s && s.Length != 0),
				});
		}
	}
}
