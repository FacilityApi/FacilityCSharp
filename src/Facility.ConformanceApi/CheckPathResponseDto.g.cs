// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using MessagePack;
using ProtoBuf;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Response for CheckPath.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[ProtoContract]
	[MessagePackObject]
	public sealed partial class CheckPathResponseDto : ServiceDto<CheckPathResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CheckPathResponseDto()
		{
		}

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(CheckPathResponseDto? other)
		{
			return other != null;
		}
	}
}
