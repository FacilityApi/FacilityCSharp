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

	public override bool IsEquivalentTo(ValueDto? other) =>
		other != null &&
		BooleanValue == other.BooleanValue &&
		StringValue == other.StringValue &&
		ServiceDataUtility.AreEquivalentFieldValues(ErrorArrayValue, other.ErrorArrayValue) &&
		ServiceDataUtility.AreEquivalentFieldValues(BooleanMapValue, other.BooleanMapValue) &&
		ServiceDataUtility.AreEquivalentFieldValues(ErrorMapValue, other.ErrorMapValue);
}
