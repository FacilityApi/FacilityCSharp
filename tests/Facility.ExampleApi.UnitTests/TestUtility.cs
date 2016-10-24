using System;
using Facility.Core;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace Facility.ExampleApi.UnitTests
{
	internal static class TestUtility
	{
		public static void ShouldBeEquivalent(this ServiceDto actual, ServiceDto expected)
		{
			if (!ServiceDataUtility.AreEquivalentDtos(actual, expected))
			{
				var actualString = actual.ToString();
				actualString.ShouldBe(expected.ToString());
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}

		public static void ShouldBeEquivalent(this ServiceResult actual, ServiceResult expected)
		{
			if (!ServiceDataUtility.AreEquivalentResults(actual, expected))
			{
				var actualString = actual.ToString();
				actualString.ShouldBe(expected.ToString());
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}

		public static void ShouldBeEquivalent(this JObject actual, JObject expected)
		{
			if (!ServiceDataUtility.AreEquivalentObjects(actual, expected))
			{
				var actualString = actual.ToString();
				actualString.ShouldBe(expected.ToString());
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}

		public static void ShouldBeEquivalent(this byte[] actual, byte[] expected)
		{
			if (!ServiceDataUtility.AreEquivalentBytes(actual, expected))
			{
				var actualString = BitConverter.ToString(actual);
				actualString.ShouldBe(BitConverter.ToString(expected));
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}
	}
}
