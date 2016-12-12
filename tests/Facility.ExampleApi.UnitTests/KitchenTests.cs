using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using NUnit.Framework;
using Shouldly;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class KitchenTests
	{
		public KitchenTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.KitchenAsync(default(KitchenRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task NotAdminErrorCode()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.KitchenAsync(new KitchenRequestDto(), CancellationToken.None)).ShouldBeFailure(ExampleApiErrors.NotAdmin);
		}

		readonly string m_category;
	}
}
