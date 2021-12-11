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
	/// Request for CheckPath.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class CheckPathRequestDto : ServiceDto<CheckPathRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CheckPathRequestDto()
		{
		}

		[MessagePack.Key("string")]
		public string? String { get; set; }

		[MessagePack.Key("boolean")]
		public bool? Boolean { get; set; }

		[MessagePack.Key("double")]
		public double? Double { get; set; }

		[MessagePack.Key("int32")]
		public int? Int32 { get; set; }

		[MessagePack.Key("int64")]
		public long? Int64 { get; set; }

		[MessagePack.Key("decimal")]
		public decimal? Decimal { get; set; }

		[MessagePack.Key("enum")]
		public Answer? Enum { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

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
