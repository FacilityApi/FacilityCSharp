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
	/// Request for DeleteWidget.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[ProtoContract]
	public sealed partial class DeleteWidgetRequestDto : ServiceDto<DeleteWidgetRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public DeleteWidgetRequestDto()
		{
		}

		/// <summary>
		/// The widget ID.
		/// </summary>
		[ProtoMember(1)]
		public int? Id { get; set; }

		/// <summary>
		/// Don't delete the widget unless it has this ETag.
		/// </summary>
		[ProtoMember(2)]
		public string? IfETag { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(DeleteWidgetRequestDto? other)
		{
			return other != null &&
				Id == other.Id &&
				IfETag == other.IfETag;
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
			if (Id != null && Id < 0)
				return ServiceDataUtility.GetInvalidFieldErrorMessage("id", "Must be at least 0.");

			return null;
		}
	}
}
