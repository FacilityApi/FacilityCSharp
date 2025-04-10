using System.Diagnostics.CodeAnalysis;
using MessagePack;

namespace Facility.Core.UnitTests;

[MessagePackObject]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "Must be public for MessagePack.")]
public sealed class ValueDto : SystemTextJsonServiceDto<ValueDto>
{
	public static ValueDto Create(bool? value) => new ValueDto { BooleanValue = value };

	public static ValueDto Create(string value) => new ValueDto { StringValue = value };

	public static ValueDto Create(IReadOnlyList<ServiceErrorDto> value) => new ValueDto { ErrorArrayValue = value };

	public static ValueDto Create(IReadOnlyDictionary<string, bool> value) => new ValueDto { BooleanMapValue = value };

	public static ValueDto Create(IReadOnlyDictionary<string, ServiceErrorDto> value) => new ValueDto { ErrorMapValue = value };

	public static ValueDto Create(int? value) => new ValueDto { IntegerValue = value };

	public static ValueDto Create(float? value) => new ValueDto { FloatValue = value };

	public static ValueDto Create(double? value) => new ValueDto { DoubleValue = value };

	public static ValueDto Create(DateTime? value) => new ValueDto { DateTimeValue = value };

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

	[Key(7)]
	public DateTime? DateTimeValue { get; set; }

	[Key(8)]
	public float? FloatValue { get; set; }
}
