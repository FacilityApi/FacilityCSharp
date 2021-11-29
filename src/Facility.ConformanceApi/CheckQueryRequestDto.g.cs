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
	/// Request for CheckQuery.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class CheckQueryRequestDto : ServiceDto<CheckQueryRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CheckQueryRequestDto()
		{
		}

		public string? String { get; set; }

		public bool? Boolean { get; set; }

		public double? Double { get; set; }

		public int? Int32 { get; set; }

		public long? Int64 { get; set; }

		public decimal? Decimal { get; set; }

		public Answer? Enum { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(CheckQueryRequestDto? other)
		{
			return other != null &&
				String == other.String &&
				Boolean == other.Boolean &&
				Double == other.Double &&
				Int32 == other.Int32 &&
				Int64 == other.Int64 &&
				Decimal == other.Decimal &&
				Enum == other.Enum;
		}

		/// <summary>
		/// Validates the DTO.
		/// </summary>
		public override bool Validate(out string? errorMessage)
		{
			errorMessage = GetValidationErrorMessage();
			return errorMessage == null;
		}

		private string? GetValidationErrorMessage()
		{
			if (Enum != null && !Enum.Value.IsDefined())
				return ServiceDataUtility.GetInvalidFieldErrorMessage("enum", "Must be an expected enum value.");

			return null;
		}
	}
}
