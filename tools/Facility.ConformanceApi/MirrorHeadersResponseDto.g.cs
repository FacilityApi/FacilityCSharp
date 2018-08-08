// DO NOT EDIT: generated by fsdgencsharp
// <auto-generated />
using System;
using System.Collections.Generic;
using Facility.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Response for MirrorHeaders.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class MirrorHeadersResponseDto : ServiceDto<MirrorHeadersResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MirrorHeadersResponseDto()
		{
		}

		public string String { get; set; }

		public bool? Boolean { get; set; }

		public double? Double { get; set; }

		public int? Int32 { get; set; }

		public long? Int64 { get; set; }

		public decimal? Decimal { get; set; }

		public Answer? Enum { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MirrorHeadersResponseDto other)
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
