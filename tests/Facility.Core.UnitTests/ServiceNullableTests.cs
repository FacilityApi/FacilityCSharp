using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

internal sealed class ServiceNullableTests
{
	[Test]
	public void StringField()
	{
		ServiceNullable<string?> field = default;
		field.IsUnspecified.Should().BeTrue();
		field.IsNull.Should().BeFalse();
		field.Value.Should().BeNull();
		field.Equals(default).Should().BeTrue();
		field.Equals(null).Should().BeFalse();
		field.Equals("").Should().BeFalse();
		(field == default).Should().BeTrue();
		(field != default).Should().BeFalse();
		Equals(field, default(ServiceNullable<string?>)).Should().BeTrue();

		field = null;
		field.IsUnspecified.Should().BeFalse();
		field.IsNull.Should().BeTrue();
		field.Value.Should().BeNull();
		field.Equals(default).Should().BeFalse();
		field.Equals(null).Should().BeTrue();
		field.Equals("").Should().BeFalse();
		(field == default).Should().BeFalse();
		(field != default).Should().BeTrue();
		Equals(field, new ServiceNullable<string?>(null)).Should().BeTrue();

		field = "";
		field.IsUnspecified.Should().BeFalse();
		field.IsNull.Should().BeFalse();
		field.Value.Should().Be("");
		field.Equals(default).Should().BeFalse();
		field.Equals(null).Should().BeFalse();
		field.Equals("").Should().BeTrue();
		(field == default).Should().BeFalse();
		(field != default).Should().BeTrue();
		Equals(field, new ServiceNullable<string?>("")).Should().BeTrue();
	}
}
