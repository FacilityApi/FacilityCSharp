// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Facility.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdgeCases
{
	/// <summary>
	/// A DTO.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class DataDto : ServiceDto<DataDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public DataDto()
		{
		}

		[JsonProperty("f")]
		public string? Field { get; set; }

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
