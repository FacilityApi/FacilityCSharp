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
	/// Request for CreateWidget.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class CreateWidgetRequestDto : ServiceDto<CreateWidgetRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CreateWidgetRequestDto()
		{
		}

		/// <summary>
		/// The widget to create.
		/// </summary>
		public WidgetDto? Widget { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(CreateWidgetRequestDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentDtos(Widget, other.Widget);
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
