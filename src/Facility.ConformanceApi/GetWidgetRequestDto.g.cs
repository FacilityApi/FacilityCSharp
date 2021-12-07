// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using MessagePack;
using ProtoBuf;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for GetWidget.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[ProtoContract]
	[MessagePackObject]
	public sealed partial class GetWidgetRequestDto : ServiceDto<GetWidgetRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public GetWidgetRequestDto()
		{
		}

		/// <summary>
		/// The widget ID.
		/// </summary>
		[ProtoMember(1)]
		[Key(0)]
		public int? Id { get; set; }

		/// <summary>
		/// Don't get the widget if it has this ETag.
		/// </summary>
		[ProtoMember(2)]
		[Key(1)]
		public string? IfNotETag { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(GetWidgetRequestDto? other)
		{
			return other != null &&
				Id == other.Id &&
				IfNotETag == other.IfNotETag;
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
			if (Id == null)
				return ServiceDataUtility.GetRequiredFieldErrorMessage("id");

			if (Id != null && Id < 0)
				return ServiceDataUtility.GetInvalidFieldErrorMessage("id", "Must be at least 0.");

			return null;
		}
	}
}
