// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using MessagePack;
using ProtoBuf;

namespace EdgeCases
{
	/// <summary>
	/// Request for OldMethod.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[ProtoContract]
	[MessagePackObject]
	public sealed partial class OldMethodRequestDto : ServiceDto<OldMethodRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public OldMethodRequestDto()
		{
		}

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(OldMethodRequestDto? other)
		{
			return other != null;
		}
	}
}
