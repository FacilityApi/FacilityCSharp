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
	/// Response for CheckQuery.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[FacilitySerializer(FacilitySerializer.SystemTextJson)]
	public sealed partial class CheckQueryResponseDto : ServiceDto<CheckQueryResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CheckQueryResponseDto()
		{
		}

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(CheckQueryResponseDto? other)
		{
			return other != null;
		}
	}
}
