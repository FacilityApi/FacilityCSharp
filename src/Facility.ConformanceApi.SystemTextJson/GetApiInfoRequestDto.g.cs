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
	/// Request for GetApiInfo.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[FacilitySerializer(FacilitySerializer.SystemTextJson)]
	public sealed partial class GetApiInfoRequestDto : ServiceDto<GetApiInfoRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public GetApiInfoRequestDto()
		{
		}

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(GetApiInfoRequestDto? other)
		{
			return other != null;
		}
	}
}
