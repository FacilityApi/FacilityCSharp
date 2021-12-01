using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

public sealed class ServiceObjectTests
{
	[Test]
	public void EquivalenceFromJObject()
	{
		var so1 = ServiceObject.Create(new JObject { ["foo"] = "bar" });
		var so2 = ServiceObject.Create(new JObject { ["foo"] = "bar" });
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void EquivalenceFromJsonObject()
	{
		var so1 = ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		var so2 = ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}
}
