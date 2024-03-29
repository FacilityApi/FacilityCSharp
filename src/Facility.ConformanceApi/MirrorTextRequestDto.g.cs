// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using Facility.Core.MessagePack;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for MirrorText.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class MirrorTextRequestDto : ServiceDto<MirrorTextRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MirrorTextRequestDto()
		{
		}

		[MessagePack.Key("content")]
		public string? Content { get; set; }

		[MessagePack.Key("type")]
		public string? Type { get; set; }

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		protected override JsonServiceSerializer JsonSerializer => SystemTextJsonServiceSerializer.Instance;

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MirrorTextRequestDto? other)
		{
			return other != null &&
				Content == other.Content &&
				Type == other.Type;
		}
	}
}
