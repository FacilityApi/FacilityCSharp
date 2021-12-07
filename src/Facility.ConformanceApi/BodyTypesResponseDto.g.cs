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
	/// Response for BodyTypes.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class BodyTypesResponseDto : ServiceDto<BodyTypesResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public BodyTypesResponseDto()
		{
		}

		public byte[]? Content { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(BodyTypesResponseDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentBytes(Content, other.Content);
		}
	}
}
