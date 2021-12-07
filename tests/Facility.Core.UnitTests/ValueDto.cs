using MessagePack;
using ProtoBuf;

namespace Facility.Core.UnitTests;

[ProtoContract]
[MessagePackObject]
public sealed class ValueDto : ServiceDto<ValueDto>
{
	public static ValueDto Create(bool? value)
	{
		return new ValueDto { BooleanValue = value };
	}

	public static ValueDto Create(string value)
	{
		return new ValueDto { StringValue = value };
	}

	public static ValueDto Create(IReadOnlyList<ServiceErrorDto> value)
	{
		return new ValueDto { ErrorArrayValue = value };
	}

	public static ValueDto Create(IReadOnlyDictionary<string, bool> value)
	{
		return new ValueDto { BooleanMapValue = value };
	}

	public static ValueDto Create(IReadOnlyDictionary<string, ServiceErrorDto> value)
	{
		return new ValueDto { ErrorMapValue = value };
	}

	[ProtoMember(1)]
	[Key(0)]
	public bool? BooleanValue { get; set; }

	[ProtoMember(2)]
	[Key(1)]
	public string? StringValue { get; set; }

	[ProtoMember(3)]
	[Key(2)]
	public IReadOnlyList<ServiceErrorDto>? ErrorArrayValue { get; set; }

	[ProtoMember(4)]
	[Key(3)]
	public IReadOnlyDictionary<string, bool>? BooleanMapValue { get; set; }

	[ProtoMember(5)]
	[Key(4)]
	public IReadOnlyDictionary<string, ServiceErrorDto>? ErrorMapValue { get; set; }

	public override bool IsEquivalentTo(ValueDto? other)
	{
		return other != null &&
			BooleanValue == other.BooleanValue &&
			StringValue == other.StringValue &&
			ServiceDataUtility.AreEquivalentFieldValues(ErrorArrayValue, other.ErrorArrayValue) &&
			ServiceDataUtility.AreEquivalentFieldValues(BooleanMapValue, other.BooleanMapValue) &&
			ServiceDataUtility.AreEquivalentFieldValues(ErrorMapValue, other.ErrorMapValue);
	}
}
