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
	public class DeleteWidgetTests
	{
		public DeleteWidgetTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.DeleteWidgetAsync(default(DeleteWidgetRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task NullId_InvalidRequest()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.DeleteWidgetAsync(id: null)).ShouldBeFailure(ServiceErrors.InvalidRequest);
		}

		[Test]
		public async Task BlankId_NotFound()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.DeleteWidgetAsync(id: "")).ShouldBeFailure(ServiceErrors.InvalidRequest);
		}

		[Test]
		public async Task NotFoundId_NotFound()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.DeleteWidgetAsync(id: "xyzzy")).ShouldBeFailure(ExampleApiErrors.CreateNotFoundWidget("xyzzy"));
		}

		[Test]
		public async Task FoundId_Widget()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			(await service.DeleteWidgetAsync(id: widget.Id)).ShouldBeSuccess(new DeleteWidgetResponseDto());
		}

		readonly string m_category;
	}
}
