// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for Required.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class RequiredRequestDto : ServiceDto<RequiredRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public RequiredRequestDto()
		{
		}

		public string? Query { get; set; }

		public string? Normal { get; set; }

		public WidgetDto? Widget { get; set; }

		public IReadOnlyList<WidgetDto>? Widgets { get; set; }

		public IReadOnlyList<IReadOnlyList<WidgetDto>>? WidgetMatrix { get; set; }

		public ServiceResult<WidgetDto>? WidgetResult { get; set; }

		public IReadOnlyList<ServiceResult<WidgetDto>>? WidgetResults { get; set; }

		public IReadOnlyDictionary<string, WidgetDto>? WidgetMap { get; set; }

		public HasWidgetDto? HasWidget { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(RequiredRequestDto? other)
		{
			return other != null &&
				Query == other.Query &&
				Normal == other.Normal &&
				ServiceDataUtility.AreEquivalentDtos(Widget, other.Widget) &&
				ServiceDataUtility.AreEquivalentFieldValues(Widgets, other.Widgets) &&
				ServiceDataUtility.AreEquivalentFieldValues(WidgetMatrix, other.WidgetMatrix) &&
				ServiceDataUtility.AreEquivalentResults(WidgetResult, other.WidgetResult) &&
				ServiceDataUtility.AreEquivalentFieldValues(WidgetResults, other.WidgetResults) &&
				ServiceDataUtility.AreEquivalentFieldValues(WidgetMap, other.WidgetMap) &&
				ServiceDataUtility.AreEquivalentDtos(HasWidget, other.HasWidget);
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
			if (Query == null)
				return ServiceDataUtility.GetRequiredFieldErrorMessage("query");
			if (Normal == null)
				return ServiceDataUtility.GetRequiredFieldErrorMessage("normal");

			string? errorMessage;
			if (!ServiceDataUtility.ValidateFieldValue(Widget, "widget", out errorMessage))
				return errorMessage!;
			if (!ServiceDataUtility.ValidateFieldValue(Widgets, "widgets", out errorMessage))
				return errorMessage!;
			if (!ServiceDataUtility.ValidateFieldValue(WidgetMatrix, "widgetMatrix", out errorMessage))
				return errorMessage!;
			if (!ServiceDataUtility.ValidateFieldValue(WidgetResult, "widgetResult", out errorMessage))
				return errorMessage!;
			if (!ServiceDataUtility.ValidateFieldValue(WidgetResults, "widgetResults", out errorMessage))
				return errorMessage!;
			if (!ServiceDataUtility.ValidateFieldValue(WidgetMap, "widgetMap", out errorMessage))
				return errorMessage!;
			if (!ServiceDataUtility.ValidateFieldValue(HasWidget, "hasWidget", out errorMessage))
				return errorMessage!;

			return null;
		}
	}
}
