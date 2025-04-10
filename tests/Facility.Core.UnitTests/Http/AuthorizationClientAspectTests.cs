#if NET472
using System.Net.Http;
#endif
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

#pragma warning disable 618 // obsolete

namespace Facility.Core.UnitTests.Http;

internal sealed class AuthorizationClientAspectTests
{
	[Test]
	public async Task AuthorizationClientAspectWorks()
	{
		const string header = "MyAuth Whatever";
		var aspect = AuthorizationClientAspect.Create(header);
		var httpRequest = new HttpRequestMessage();
		await aspect.RequestReadyAsync(httpRequest, new TestDto(), CancellationToken.None);
		httpRequest.Headers.Authorization!.ToString().Should().Be(header);
	}
}
