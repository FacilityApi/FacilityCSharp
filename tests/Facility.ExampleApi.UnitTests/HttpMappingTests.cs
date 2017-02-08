using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Assertions;
using Facility.Core.Http;
using Facility.ExampleApi.InMemory;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class HttpMappingTests
	{
		[Test]
		public async Task GetWidgetsQuery_UsesShortQueryParameter()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.GetAsync("http://local.example.com/v1/widgets?q=" + InMemoryExampleApiRepository.SampleWidgets[1].Name);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			(await JsonHttpContentSerializer.Instance.ReadHttpContentAsync<GetWidgetsResponseDto>(response.Content)).Value.Widgets[0].Should().BeDto(InMemoryExampleApiRepository.SampleWidgets[1]);
		}

		[Test]
		public async Task GetExistingWidget_Uses200()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.GetAsync("http://local.example.com/v1/widgets/" + InMemoryExampleApiRepository.SampleWidgets[0].Id);
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			(await JsonHttpContentSerializer.Instance.ReadHttpContentAsync<WidgetDto>(response.Content)).Value.Should().BeDto(InMemoryExampleApiRepository.SampleWidgets[0]);
		}

		[Test]
		public async Task GetMissingWidget_Uses404()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.GetAsync("http://local.example.com/v1/widgets/xyzzy");
			response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public async Task CreateWidget_Uses201()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var widget = new WidgetDto();
			var response = await httpClient.PostAsync("http://local.example.com/v1/widgets", JsonHttpContentSerializer.Instance.CreateHttpContent(widget));
			response.StatusCode.Should().Be(HttpStatusCode.Created);
			(await JsonHttpContentSerializer.Instance.ReadHttpContentAsync<WidgetDto>(response.Content)).Value.Id.Should().NotBeNull();
		}

		[Test]
		public async Task DeleteExistingWidget_Uses204()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.DeleteAsync("http://local.example.com/v1/widgets/" + InMemoryExampleApiRepository.SampleWidgets[0].Id);
			response.StatusCode.Should().Be(HttpStatusCode.NoContent);
		}

		[Test]
		public async Task DeleteMissingWidget_Uses404()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.DeleteAsync("http://local.example.com/v1/widgets/xyzzy");
			response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}

		[Test]
		public async Task HtmlNotJson_BadRequest()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.PostAsync("http://local.example.com/v1/widgets", new StringContent("<html></html>", Encoding.UTF8, HttpServiceUtility.JsonMediaType));
			response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
			(await JsonHttpContentSerializer.Instance.ReadHttpContentAsync<ServiceErrorDto>(response.Content)).Value.Code.Should().Be(ServiceErrors.InvalidRequest);
		}

		[Test]
		public async Task BadJsonSyntax_BadRequest()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.PostAsync("http://local.example.com/v1/widgets", new StringContent("{\"name\":\"xyzzy\"", Encoding.UTF8, HttpServiceUtility.JsonMediaType));
			response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
			(await JsonHttpContentSerializer.Instance.ReadHttpContentAsync<ServiceErrorDto>(response.Content)).Value.Code.Should().Be(ServiceErrors.InvalidRequest);
		}

		[Test]
		public async Task UnexpectedJson_BadRequest()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.PostAsync("http://local.example.com/v1/widgets", new StringContent("{\"name\":{}}", Encoding.UTF8, HttpServiceUtility.JsonMediaType));
			response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
			(await JsonHttpContentSerializer.Instance.ReadHttpContentAsync<ServiceErrorDto>(response.Content)).Value.Code.Should().Be(ServiceErrors.InvalidRequest);
		}

		[Test]
		public async Task FieldNotInTheSpec_Success()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.PostAsync("http://local.example.com/v1/widgets", new StringContent("{\"name\":\"xyzzy\",\"notInTheSpec\":42}", Encoding.UTF8, HttpServiceUtility.JsonMediaType));
			response.StatusCode.Should().Be(HttpStatusCode.Created);
		}

		[Test]
		public async Task NotAdminError_Uses403()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.PostAsync("http://local.example.com/v1/kitchen", new StringContent("{}", Encoding.UTF8, HttpServiceUtility.JsonMediaType));
			response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		}

		[Test]
		public async Task MissingBody_BadRequest()
		{
			var httpClient = TestUtility.CreateTestHttpClient();
			var response = await httpClient.PostAsync("http://local.example.com/v1/kitchen", new ByteArrayContent(new byte[0]));
			response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		}
	}
}
