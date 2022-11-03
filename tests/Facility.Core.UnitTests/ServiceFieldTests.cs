using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

public class ServiceFieldTests
{
	[Test]
	public void StringField()
	{
		ServiceField<string?> field = default;
		field.IsDefault.Should().BeTrue();
		field.IsNull.Should().BeFalse();
		field.Value.Should().BeNull();
		field.Equals(default).Should().BeTrue();
		field.Equals(null).Should().BeFalse();
		field.Equals("").Should().BeFalse();
		(field == default).Should().BeTrue();
		(field != default).Should().BeFalse();
		Equals(field, default(ServiceField<string?>)).Should().BeTrue();

		field = null;
		field.IsDefault.Should().BeFalse();
		field.IsNull.Should().BeTrue();
		field.Value.Should().BeNull();
		field.Equals(default).Should().BeFalse();
		field.Equals(null).Should().BeTrue();
		field.Equals("").Should().BeFalse();
		(field == default).Should().BeFalse();
		(field != default).Should().BeTrue();
		Equals(field, new ServiceField<string?>(null)).Should().BeTrue();

		field = "";
		field.IsDefault.Should().BeFalse();
		field.IsNull.Should().BeFalse();
		field.Value.Should().Be("");
		field.Equals(default).Should().BeFalse();
		field.Equals(null).Should().BeFalse();
		field.Equals("").Should().BeTrue();
		(field == default).Should().BeFalse();
		(field != default).Should().BeTrue();
		Equals(field, new ServiceField<string?>("")).Should().BeTrue();
	}
}
