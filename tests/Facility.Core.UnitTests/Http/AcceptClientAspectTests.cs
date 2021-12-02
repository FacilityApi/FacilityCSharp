using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

#pragma warning disable 618 // obsolete

namespace Facility.Core.UnitTests.Http
{
	public class AcceptClientAspectTests
	{
		[Test]
		public async Task AcceptClientAspectWorks()
		{
			const string header = HttpServiceUtility.JsonMediaType;
			var aspect = AcceptClientAspect.Create(header);
			var httpRequest = new HttpRequestMessage();
			await aspect.RequestReadyAsync(httpRequest, new TestDto(), CancellationToken.None);
			httpRequest.Headers.Accept.ToString().Should().Be(header);
		}

		private class TestDto : ServiceDto<TestDto>
		{
			public override bool IsEquivalentTo(TestDto? other)
			{
				return other != null;
			}
		}
	}
}
