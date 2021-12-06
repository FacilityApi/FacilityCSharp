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
	/// Response for MirrorFields.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class MirrorFieldsResponseDto : ServiceDto<MirrorFieldsResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MirrorFieldsResponseDto()
		{
		}

		public AnyDto? Field { get; set; }

		public IReadOnlyList<IReadOnlyList<IReadOnlyList<double>>>? Matrix { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToString(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MirrorFieldsResponseDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentDtos(Field, other.Field) &&
				ServiceDataUtility.AreEquivalentFieldValues(Matrix, other.Matrix);
		}
	}
}
