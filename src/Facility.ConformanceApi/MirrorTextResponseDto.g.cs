// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Response for MirrorText.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class MirrorTextResponseDto : ServiceDto<MirrorTextResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MirrorTextResponseDto()
		{
		}

		public string? Content { get; set; }

		public string? Type { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToString(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MirrorTextResponseDto? other)
		{
			return other != null &&
				Content == other.Content &&
				Type == other.Type;
		}
	}
}
