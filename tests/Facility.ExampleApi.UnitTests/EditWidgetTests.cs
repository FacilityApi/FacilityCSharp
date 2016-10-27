using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Facility.ExampleApi.InMemory;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public sealed class EditWidgetTests
	{
		public EditWidgetTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.EditWidgetAsync(default(EditWidgetRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task NoOp_Widget()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			(await service.EditWidgetAsync(new EditWidgetRequestDto { Id = widget.Id }, CancellationToken.None))
				.ShouldBeSuccess(new EditWidgetResponseDto { Widget = widget });
		}

		[Test]
		public async Task Op_Job()
		{
			var service = TestUtility.CreateService(m_category);
			var widget = InMemoryExampleApiRepository.SampleWidgets[0];
			(await service.EditWidgetAsync(new EditWidgetRequestDto { Id = widget.Id, Ops = new[] { new JObject() } }, CancellationToken.None))
				.ShouldBeSuccess(new EditWidgetResponseDto { Job = new WidgetJobDto { Id = "TODO" } });
		}

		readonly string m_category;
	}
}
