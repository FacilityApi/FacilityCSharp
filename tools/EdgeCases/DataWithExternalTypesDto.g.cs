// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using Facility.Core.MessagePack;

namespace EdgeCases
{
	/// <summary>
	/// A DTO that uses external types
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class DataWithExternalTypesDto : ServiceDto<DataWithExternalTypesDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public DataWithExternalTypesDto()
		{
		}

		[MessagePack.Key("foo")]
		public EdgeCases.ExternTypes.ExternalDataDto? Foo { get; set; }

		[MessagePack.Key("bar")]
		public EdgeCases.ExternTypes.ExternalEnum? Bar { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(DataWithExternalTypesDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentDtos(Foo, other.Foo) &&
				Bar == other.Bar;
		}
	}
}
