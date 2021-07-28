// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Facility.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for GetWidgetBatch.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class GetWidgetBatchRequestDto : ServiceDto<GetWidgetBatchRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public GetWidgetBatchRequestDto()
		{
		}

		/// <summary>
		/// The IDs of the widgets to return.
		/// </summary>
		public IReadOnlyList<int>? Ids { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(GetWidgetBatchRequestDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentFieldValues(Ids, other.Ids);
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
			if (Ids == null)
				return ServiceDataUtility.GetRequiredFieldErrorMessage("ids");

			return null;
		}
	}
}
