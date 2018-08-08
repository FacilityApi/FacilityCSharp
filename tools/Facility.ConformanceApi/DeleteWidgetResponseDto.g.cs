// DO NOT EDIT: generated by fsdgencsharp
// <auto-generated />
using System;
using System.Collections.Generic;
using Facility.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Response for DeleteWidget.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class DeleteWidgetResponseDto : ServiceDto<DeleteWidgetResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public DeleteWidgetResponseDto()
		{
		}

		/// <summary>
		/// The widget was not found.
		/// </summary>
		public bool? NotFound { get; set; }

		/// <summary>
		/// The widget no longer has the specified ETag.
		/// </summary>
		public bool? Conflict { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(DeleteWidgetResponseDto other)
		{
			return other != null &&
				NotFound == other.NotFound &&
				Conflict == other.Conflict;
		}
	}
}
