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
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[ProtoContract]
	public sealed partial class HasWidgetDto : ServiceDto<HasWidgetDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public HasWidgetDto()
		{
		}

		[ProtoMember(1)]
		public WidgetDto? Widget { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(HasWidgetDto? other)
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
