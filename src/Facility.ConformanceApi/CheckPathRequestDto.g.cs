// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using ProtoBuf;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for CheckPath.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[ProtoContract]
	public sealed partial class CheckPathRequestDto : ServiceDto<CheckPathRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CheckPathRequestDto()
		{
		}

		[ProtoMember(1)]
		public string? String { get; set; }

		[ProtoMember(2)]
		public bool? Boolean { get; set; }

		[ProtoMember(3)]
		public double? Double { get; set; }

		[ProtoMember(4)]
		public int? Int32 { get; set; }

		[ProtoMember(5)]
		public long? Int64 { get; set; }

		[ProtoMember(6)]
		public decimal? Decimal { get; set; }

		[ProtoMember(7)]
		public Answer? Enum { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(CheckPathRequestDto? other)
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
