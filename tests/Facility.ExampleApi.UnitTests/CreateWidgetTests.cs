using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public sealed class CreateWidgetTests
	{
		public CreateWidgetTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.CreateWidgetAsync(default(CreateWidgetRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task NamedWidget_Success()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = new WidgetDto(name: "Test Widget");
			var newWidget = (await service.CreateWidgetAsync(widget)).Value.Widget;
			newWidget.Id.ShouldNotBe(null);
			widget = new WidgetDto(id: newWidget.Id, name: widget.Name);
			newWidget.ShouldBeEquivalent(widget);
		}

		[Test]
		public async Task EmptyWidget_Success()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = new WidgetDto();
			var newWidget = (await service.CreateWidgetAsync(widget)).Value.Widget;
			newWidget.Id.ShouldNotBe(null);
			widget = new WidgetDto(id: newWidget.Id, name: widget.Name);
			newWidget.ShouldBeEquivalent(widget);
		}

		[Test]
		public async Task MissingWidget_SameAsEmpty()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.CreateWidgetAsync(widget: null)).ShouldBeSuccess();
		}

		readonly string m_category;
	}
}
