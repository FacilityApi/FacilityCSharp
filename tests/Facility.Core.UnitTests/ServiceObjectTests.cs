using System.Text.Json.Nodes;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

public sealed class ServiceObjectTests
{
	[Test]
	public void Equivalence([Values] bool legacy1, [Values] bool legacy2)
	{
		var so1 = legacy1 ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		var so2 = legacy1 ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void Null([Values] bool legacy)
	{
		(legacy ? ServiceObject.Create(default(JObject)) : ServiceObject.Create(default(JsonObject))).Should().BeNull();
	}

	[Test]
	public void Clone([Values] bool legacy1, [Values] bool legacy2)
	{
		var so = legacy1 ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });

		if (legacy2)
			so.AsJObject(ServiceObjectAccess.Clone)["foo"] = "baz";
		else
			so.AsJsonObject(ServiceObjectAccess.Clone)["foo"] = "baz";

		so.ToString().Should().Be(@"{""foo"":""bar""}");
	}

	[Test]
	public void ReadOnly([Values] bool legacy1, [Values] bool legacy2)
	{
		var so = legacy1 ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });

		if (legacy2)
			so.AsJObject(ServiceObjectAccess.ReadOnly)["foo"].Value<string>().Should().Be("bar");
		else
			so.AsJsonObject(ServiceObjectAccess.ReadOnly)["foo"]!.GetValue<string>().Should().Be("bar");
	}

	[Test]
	public void ReadWrite([Values] bool legacy1, [Values] bool legacy2, [Values] bool legacy3)
	{
		var so = legacy1 ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });

		if (legacy2)
			so.AsJObject(ServiceObjectAccess.ReadWrite)["foo"] = "baz";
		else
			so.AsJsonObject(ServiceObjectAccess.ReadWrite)["foo"] = "baz";

		if (legacy3)
			so.AsJObject(ServiceObjectAccess.ReadWrite)["foo"] = "buz";
		else
			so.AsJsonObject(ServiceObjectAccess.ReadWrite)["foo"] = "buz";

		so.ToString().Should().Be(@"{""foo"":""buz""}");
	}
}
