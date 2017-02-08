using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core.Assertions;
using Facility.ExampleApi.InMemory;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class GetWidgetsTests
	{
		public GetWidgetsTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetWidgetsAsync(default(GetWidgetsRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task NullQuery_AllWidgets()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetsAsync(query: null)).Should().BeSuccess(
				new GetWidgetsResponseDto
				{
					Widgets = InMemoryExampleApiRepository.SampleWidgets.OrderBy(x => x.Id).ToList(),
					Total = InMemoryExampleApiRepository.SampleWidgets.Count,
				});
		}

		[Test]
		public async Task BlankQuery_NoWidgets()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetsAsync(query: "")).Should().BeSuccess(
				new GetWidgetsResponseDto
				{
					Widgets = new WidgetDto[0],
					Total = 0,
				});
		}

		[Test]
		public async Task NotFoundQuery_NoWidgets()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetsAsync(query: "xyzzy")).Should().BeSuccess(
				new GetWidgetsResponseDto
				{
					Widgets = new WidgetDto[0],
					Total = 0,
				});
		}

		[Test]
		public async Task FoundQuery_Widgets()
		{
			var service = TestUtility.CreateService(m_category);
			var widgets = InMemoryExampleApiRepository.SampleWidgets.Where(x => x.Name.Contains("ey")).OrderBy(x => x.Id).ToList();
			(await service.GetWidgetsAsync(query: "ey")).Should().BeSuccess(
				new GetWidgetsResponseDto
				{
					Widgets = widgets,
					Total = widgets.Count,
				});
		}

		[Test]
		public async Task ReverseSortByName()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.GetWidgetsAsync(limit: 2, sort: WidgetField.Name, desc: true)).Should().BeSuccess(
				new GetWidgetsResponseDto
				{
					Widgets = InMemoryExampleApiRepository.SampleWidgets.OrderByDescending(x => x.Name).Take(2).ToList(),
					Total = InMemoryExampleApiRepository.SampleWidgets.Count,
				});
		}

		readonly string m_category;
	}
}
