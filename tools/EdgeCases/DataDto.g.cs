// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;

namespace EdgeCases
{
	/// <summary>
	/// A DTO.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class DataDto : ServiceDto<DataDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public DataDto()
		{
		}

		[Newtonsoft.Json.JsonProperty("f")]
		[System.Text.Json.Serialization.JsonPropertyName("f")]
		[MessagePack.Key("f")]
		public string? Field { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(DataDto? other)
		{
			return other != null &&
				Field == other.Field;
		}
	}
}
