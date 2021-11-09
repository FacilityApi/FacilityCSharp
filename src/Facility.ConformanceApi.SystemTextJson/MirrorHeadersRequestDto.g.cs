// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Facility.Core;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for MirrorHeaders.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[FacilitySerializer(FacilitySerializer.SystemTextJson)]
	public sealed partial class MirrorHeadersRequestDto : ServiceDto<MirrorHeadersRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MirrorHeadersRequestDto()
		{
		}

		public string? String { get; set; }

		public bool? Boolean { get; set; }

		public double? Double { get; set; }

		public int? Int32 { get; set; }

		public long? Int64 { get; set; }

		public decimal? Decimal { get; set; }

		public Answer? Enum { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MirrorHeadersRequestDto? other)
		{
			return other != null &&
				String == other.String &&
				Boolean == other.Boolean &&
				Double == other.Double &&
				Int32 == other.Int32 &&
				Int64 == other.Int64 &&
				Decimal == other.Decimal &&
				Enum == other.Enum;
		}
	}
}
