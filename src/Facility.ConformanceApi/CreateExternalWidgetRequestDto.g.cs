// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using Facility.Core.MessagePack;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// Request for CreateExternalWidget.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class CreateExternalWidgetRequestDto : ServiceDto<CreateExternalWidgetRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public CreateExternalWidgetRequestDto()
		{
		}

		/// <summary>
		/// The external widget to create.
		/// </summary>
		[MessagePack.Key("externalWidget")]
		public Facility.ConformanceApi.External.TestExternalDto? ExternalWidget { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(CreateExternalWidgetRequestDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentDtos(ExternalWidget, other.ExternalWidget);
		}
	}
}
