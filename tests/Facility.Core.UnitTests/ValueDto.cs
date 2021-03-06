using System.Collections.Generic;

namespace Facility.Core.UnitTests
{
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

		public bool? BooleanValue { get; set; }

		public string? StringValue { get; set; }

		public IReadOnlyList<ServiceErrorDto>? ErrorArrayValue { get; set; }

		public IReadOnlyDictionary<string, bool>? BooleanMapValue { get; set; }

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
}
