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
	public void AsData([Values] bool legacy1, [Values] bool legacy2, [Values] bool legacy3, [Values] bool legacy4)
	{
		var so = legacy1 ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });

		if (legacy2)
			so.AsJObject()["foo"].Value<string>().Should().Be("bar");
		else
			so.AsJsonObject()["foo"]!.GetValue<string>().Should().Be("bar");

		if (legacy3)
			so.AsJObject()["foo"] = "baz";
		else
			so.AsJsonObject()["foo"] = "baz";

		if (legacy4)
			so.AsJObject()["foo"] = "buz";
		else
			so.AsJsonObject()["foo"] = "buz";

		so.ToString().Should().Be(@"{""foo"":""buz""}");
	}

	[Test]
	public void ToData([Values] bool legacy1, [Values] bool legacy2, [Values] bool legacy3)
	{
		var so = legacy1 ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });

		if (legacy2)
			so.ToJObject()["foo"].Value<string>().Should().Be("bar");
		else
			so.ToJsonObject()["foo"]!.GetValue<string>().Should().Be("bar");

		if (legacy3)
			so.ToJObject()["foo"] = "baz";
		else
			so.ToJsonObject()["foo"] = "baz";

		so.ToString().Should().Be(@"{""foo"":""bar""}");
	}
}
