using System;

namespace Facility.Core
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class FacilitySerializerAttribute : Attribute
	{
		public FacilitySerializerAttribute(FacilitySerializer value)
		{
			Value = value;
		}

		public FacilitySerializer Value { get; }
	}
}
