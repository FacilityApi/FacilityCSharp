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
	/// Response for CreateWidget.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class CreateWidgetResponseDto : ServiceDto<CreateWidgetResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CreateWidgetResponseDto()
		{
		}

		/// <summary>
		/// The created widget.
		/// </summary>
		[MessagePack.Key("widget")]
		public WidgetDto? Widget { get; set; }

		/// <summary>
		/// The URL of the created widget.
		/// </summary>
		[MessagePack.Key("url")]
		public string? Url { get; set; }

		/// <summary>
		/// The ETag of the created widget.
		/// </summary>
		[MessagePack.Key("eTag")]
		public string? ETag { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(CreateWidgetResponseDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentDtos(Widget, other.Widget) &&
				Url == other.Url &&
				ETag == other.ETag;
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
