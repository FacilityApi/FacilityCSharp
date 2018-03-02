using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[TestFixture("InMemory")]
	[TestFixture("TestHttpClient")]
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public sealed class TransformTests
	{
		public TransformTests(string category)
		{
			m_category = category;
		}

		[Test]
		public void NullRequest_ThrowsArgumentNullException()
		{
			var service = TestUtility.CreateService(m_category);
			Assert.ThrowsAsync<ArgumentNullException>(async () => await service.TransformAsync(default(TransformRequestDto), CancellationToken.None));
		}

		[Test]
		public async Task BeforeAfter_Success()
		{
			var service = TestUtility.CreateService(m_category);
			var before = new JObject { ["Meaning"] = 42 };
			var after = (await service.TransformAsync(new TransformRequestDto { Before = before }, CancellationToken.None)).Value.After;
			JToken.DeepEquals(before, after).Should().BeTrue();
		}

		readonly string m_category;
	}
}
