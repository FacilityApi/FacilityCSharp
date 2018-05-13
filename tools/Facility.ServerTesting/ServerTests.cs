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

		public async Task GetApiInfo()
		{
			await VerifyResponseAsync(HttpMethod.Get, "",
				HttpStatusCode.OK,
				json: new Dictionary<string, object>
				{
					["service"] = "TestServerApi",
					["version"] = "0.1.0",
				});
		}

		public async Task DeleteApiInfo()
		{
			await VerifyResponseAsync(HttpMethod.Delete, "",
				HttpStatusCode.NotFound,
				anyContent: true);
		}

		public async Task CreateWidget()
		{
			await VerifyResponseAsync(HttpMethod.Post, "/widgets",
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
					["Location"] = "/widgets/1337",
					["ETag"] = "\"initial\"",
				});
		}

		public async Task CreateWidget_NoRequestBody()
		{
			await VerifyResponseAsync(HttpMethod.Post, "/widgets",
				HttpStatusCode.BadRequest,
				json: new Dictionary<string, object>
				{
					["code"] = "InvalidRequest",
					["message"] = Matches(x => x is string s && s.Length != 0),
				});
		}

		public async Task CreateWidget_IdSpecified()
		{
			await VerifyResponseAsync(HttpMethod.Post, "/widgets",
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
			await VerifyResponseAsync(HttpMethod.Post, "/widgets",
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

		public async Task GetWidget()
		{
			await VerifyResponseAsync(HttpMethod.Get, "/widgets/1337",
				HttpStatusCode.OK,
				headers: new Dictionary<string, string>
				{
					["ETag"] = "\"initial\"",
				},
				json: new Dictionary<string, object>
				{
					["id"] = 1337,
					["name"] = "shiny",
				});
		}

		public async Task GetWidget_NotFound()
		{
			await VerifyResponseAsync(HttpMethod.Get, "/widgets/1336",
				HttpStatusCode.NotFound,
				json: new Dictionary<string, object>
				{
					["code"] = "NotFound",
					["message"] = Matches(x => x is string s && s.Length != 0),
				});
		}

		public async Task GetWidget_NotModified()
		{
			await VerifyResponseAsync(HttpMethod.Get, "/widgets/1337",
				new Dictionary<string, string>
				{
					["If-None-Match"] = "\"initial\"",
				},
				null,
				HttpStatusCode.NotModified);
		}
	}
}
