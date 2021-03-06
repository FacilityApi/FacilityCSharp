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
	/// Response for GetWidgetBatch.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class GetWidgetBatchResponseDto : ServiceDto<GetWidgetBatchResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public GetWidgetBatchResponseDto()
		{
		}

		/// <summary>
		/// The widget results.
		/// </summary>
		public IReadOnlyList<ServiceResult<WidgetDto>>? Results { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(GetWidgetBatchResponseDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentFieldValues(Results, other.Results);
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
			if (!ServiceDataUtility.ValidateFieldValue(Results, "results", out errorMessage))
				return errorMessage!;

			return null;
		}
	}
}
