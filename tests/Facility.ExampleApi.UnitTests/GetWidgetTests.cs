using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Http;
using Facility.ExampleApi.InMemory;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class GetWidgetTests
	{
		public GetWidgetTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetWidgetAsync(default(GetWidgetRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task NullId_InvalidRequest()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetAsync(id: null)).ShouldBeFailure(ServiceErrors.InvalidRequest);
		}

		[Test]
		public async Task BlankId_NotFound()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetAsync(id: "")).ShouldBeFailure(ServiceErrors.InvalidRequest);
		}

		[Test]
		public async Task NotFoundId_NotFound()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetAsync(id: "xyzzy")).ShouldBeFailure(ExampleApiErrors.CreateNotFoundWidget("xyzzy"));
		}

		[Test]
		public async Task FoundId_Widget()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			string eTag = ExampleApiService.CreateWidgetETag(widget);
			(await service.GetWidgetAsync(id: widget.Id))
				.ShouldBeSuccess(new GetWidgetResponseDto { Widget = widget, ETag = eTag });
		}

		[Test]
		public async Task NotModified_Widget()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			string eTag = ExampleApiService.CreateWidgetETag(widget);
			(await service.GetWidgetAsync(new GetWidgetRequestDto { Id = widget.Id, IfNoneMatch = eTag }, CancellationToken.None))
				.ShouldBeSuccess(new GetWidgetResponseDto { NotModified = true, ETag = eTag });
		}

		[Test]
		public async Task Modified_Widget()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			string eTag = ExampleApiService.CreateWidgetETag(widget);
			(await service.GetWidgetAsync(new GetWidgetRequestDto { Id = widget.Id, IfNoneMatch = "\"xyzzy\"" }, CancellationToken.None))
				.ShouldBeSuccess(new GetWidgetResponseDto { Widget = widget, ETag = eTag });
		}

		[Test]
		public async Task BadIfNoneMatch()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			string eTag = ExampleApiService.CreateWidgetETag(widget);
			var result = await service.GetWidgetAsync(new GetWidgetRequestDto { Id = widget.Id, IfNoneMatch = "xyzzy" }, CancellationToken.None);
			if (m_category == "InMemory")
				result.ShouldBeSuccess(new GetWidgetResponseDto { Widget = widget, ETag = eTag });
			else
				result.ShouldBeFailure(HttpServiceErrors.CreateRequestHeaderInvalidFormat("If-None-Match"));
		}

		readonly string m_category;
	}
}
