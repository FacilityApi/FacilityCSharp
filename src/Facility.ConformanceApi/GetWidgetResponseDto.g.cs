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
	/// Response for GetWidget.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[ProtoContract]
	[MessagePackObject]
	public sealed partial class GetWidgetResponseDto : ServiceDto<GetWidgetResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public GetWidgetResponseDto()
		{
		}

		/// <summary>
		/// The requested widget.
		/// </summary>
		[ProtoMember(1)]
		[Key(0)]
		public WidgetDto? Widget { get; set; }

		/// <summary>
		/// The ETag of the widget.
		/// </summary>
		[ProtoMember(2)]
		[Key(1)]
		public string? ETag { get; set; }

		/// <summary>
		/// The widget still has the specified ETag.
		/// </summary>
		[ProtoMember(3)]
		[Key(2)]
		public bool? NotModified { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(GetWidgetResponseDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentDtos(Widget, other.Widget) &&
				ETag == other.ETag &&
				NotModified == other.NotModified;
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
			string? errorMessage;
			if (!ServiceDataUtility.ValidateFieldValue(Widget, "widget", out errorMessage))
				return errorMessage!;

			return null;
		}
	}
}
