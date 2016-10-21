using System;
using Shouldly;

namespace Facility.Core.UnitTests
{
	internal static class TestUtility
	{
		public static void ShouldBe(this object actual, IServiceData expected)
		{
			if (actual != null)
			{
				actual.ShouldBeAssignableTo<IServiceData>();

				if (!ServiceDataUtility.AreEquivalent(actual, expected))
				{
					actual.ToString().ShouldBe(expected.ToString());
					throw new InvalidOperationException("Non-equivalent objects have same string representation.");
				}
			}
			else
			{
				expected.ShouldBeNull();
			}
		}
	}
}
