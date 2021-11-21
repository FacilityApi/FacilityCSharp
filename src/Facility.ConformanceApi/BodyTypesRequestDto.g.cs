// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for BodyTypes.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class BodyTypesRequestDto : ServiceDto<BodyTypesRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public BodyTypesRequestDto()
		{
		}

		public string? Content { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(BodyTypesRequestDto? other)
		{
			return other != null &&
				Content == other.Content;
		}
	}
}