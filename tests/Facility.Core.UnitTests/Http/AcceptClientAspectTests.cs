using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core.Http;
using NUnit.Framework;
using Shouldly;

namespace Facility.Core.UnitTests.Http
{
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class AcceptClientAspectTests
	{
		[Test]
		public async Task AcceptClientAspectWorks()
		{
			const string header = HttpServiceUtility.JsonMediaType;
			var aspect = AcceptClientAspect.Create(header);
			var httpRequest = new HttpRequestMessage();
			await aspect.RequestReadyAsync(httpRequest, new TestDto(), CancellationToken.None);
			httpRequest.Headers.Accept.ToString().ShouldBe(header);
		}

		private class TestDto : ServiceDto<TestDto>
		{
			public override bool IsEquivalentTo(TestDto other)
			{
				return other != null;
			}
		}
	}
}
