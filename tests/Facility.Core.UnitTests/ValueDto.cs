using MessagePack;

namespace Facility.Core.UnitTests;

[MessagePackObject]
public sealed class ValueDto : ServiceDto<ValueDto>
{
	public static ValueDto Create(bool? value) => new ValueDto { BooleanValue = value };

	public static ValueDto Create(string value) => new ValueDto { StringValue = value };

	public static ValueDto Create(IReadOnlyList<ServiceErrorDto> value) => new ValueDto { ErrorArrayValue = value };

	public static ValueDto Create(IReadOnlyDictionary<string, bool> value) => new ValueDto { BooleanMapValue = value };

	public static ValueDto Create(IReadOnlyDictionary<string, ServiceErrorDto> value) => new ValueDto { ErrorMapValue = value };

	public static ValueDto Create(int? value) => new ValueDto { IntegerValue = value };

	public static ValueDto Create(double? value) => new ValueDto { DoubleValue = value };

	[Key(0)]
	public bool? BooleanValue { get; set; }

	[Key(1)]
	public string? StringValue { get; set; }

	[Key(2)]
	public IReadOnlyList<ServiceErrorDto>? ErrorArrayValue { get; set; }

	[Key(3)]
	public IReadOnlyDictionary<string, bool>? BooleanMapValue { get; set; }

	[Key(4)]
	public IReadOnlyDictionary<string, ServiceErrorDto>? ErrorMapValue { get; set; }

	[Key(5)]
	public int? IntegerValue { get; set; }

	[Key(6)]
	public double? DoubleValue { get; set; }

	public override bool IsEquivalentTo(ValueDto? other) =>
		other != null &&
		BooleanValue == other.BooleanValue &&
		StringValue == other.StringValue &&
		ServiceDataUtility.AreEquivalentFieldValues(ErrorArrayValue, other.ErrorArrayValue) &&
		ServiceDataUtility.AreEquivalentFieldValues(BooleanMapValue, other.BooleanMapValue) &&
		ServiceDataUtility.AreEquivalentFieldValues(ErrorMapValue, other.ErrorMapValue) &&
		IntegerValue == other.IntegerValue &&
		DoubleValue.Equals(other.DoubleValue);
}
