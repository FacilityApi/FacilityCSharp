using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Assertions;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class NotRestfulTests
	{
		public NotRestfulTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.NotRestfulAsync(default(NotRestfulRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task NullId_InvalidRequest()
		{
			var service = TestUtility.CreateService(m_category);
			(await service.NotRestfulAsync(new NotRestfulRequestDto(), CancellationToken.None)).Should().BeFailure(ServiceErrors.NotFound);
		}

		readonly string m_category;
	}
}
