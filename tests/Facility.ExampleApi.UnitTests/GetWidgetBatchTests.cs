using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.ExampleApi.InMemory;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class GetWidgetBatchTests
	{
		public GetWidgetBatchTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetWidgetBatchAsync(default(GetWidgetBatchRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task MissingIds_InvalidRequest()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetBatchAsync(ids: null)).ShouldBeFailure(ServiceErrors.CreateRequestFieldRequired("ids"));
		}

		[Test]
		public async Task EmptyIds_InvalidRequest()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetBatchAsync(ids: new string[0])).ShouldBeFailure(ExampleApiErrors.CreateInvalidRequestMissingWidgetIds());
		}

		[Test]
		public async Task BlankId_Null()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetBatchAsync(new[] { "" })).ShouldBeSuccess(
				new GetWidgetBatchResponseDto
				{
					Results = new[] { ServiceResult.Failure(ExampleApiErrors.CreateInvalidRequestMissingWidgetId()).Cast<WidgetDto>() }
				});
		}

		[Test]
		public async Task NotFoundId_Null()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetBatchAsync(new[] { "xyzzy" })).ShouldBeSuccess(
				new GetWidgetBatchResponseDto
				{
					Results = new[] { ServiceResult.Failure(ExampleApiErrors.CreateNotFoundWidget("xyzzy")).Cast<WidgetDto>() }
				});
		}

		[Test]
		public async Task FoundId_Widget()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			(await service.GetWidgetBatchAsync(new[] { widget.Id })).ShouldBeSuccess(
				new GetWidgetBatchResponseDto
				{
					Results = new[] { ServiceResult.Success(widget) }
				});
		}

		[Test]
		public async Task MixedFoundAndNotFoundWidgets_WidgetsInOrder()
		{
			var service = TestUtility.CreateService(m_category);
			var widget1 = InMemoryExampleApiRepository.SampleWidgets[0];
			var widget2 = InMemoryExampleApiRepository.SampleWidgets[1];
			(await service.GetWidgetBatchAsync(new[] { widget2.Id, "xyzzy", widget1.Id })).ShouldBeSuccess(
				new GetWidgetBatchResponseDto
				{
					Results = new[]
					{
						ServiceResult.Success(widget2),
						ServiceResult.Failure(ExampleApiErrors.CreateNotFoundWidget("xyzzy")).Cast<WidgetDto>(),
						ServiceResult.Success(widget1)
					}
				});
		}

		readonly string m_category;
	}
}
