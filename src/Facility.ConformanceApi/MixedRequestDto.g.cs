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
	/// Request for Mixed.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class MixedRequestDto : ServiceDto<MixedRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MixedRequestDto()
		{
		}

		[MessagePack.Key("path")]
		public string? Path { get; set; }

		[MessagePack.Key("query")]
		public string? Query { get; set; }

		[MessagePack.Key("header")]
		public string? Header { get; set; }

		[MessagePack.Key("normal")]
		public string? Normal { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MixedRequestDto? other)
		{
			return other != null &&
				Path == other.Path &&
				Query == other.Query &&
				Header == other.Header &&
				Normal == other.Normal;
		}
	}
}
