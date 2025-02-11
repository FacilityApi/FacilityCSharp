// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;
using Facility.Core.MessagePack;

namespace EdgeCases
{
	/// <summary>
	/// Request for Misc.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class MiscRequestDto : ServiceDto<MiscRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MiscRequestDto()
		{
		}

		[MessagePack.Key("q")]
		public string? Q { get; set; }

		[Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Include)]
		[ServiceNullableDefaultValueAttribute(typeof(ServiceNullable<string?>))]
		[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
		[MessagePack.Key("f")]
		public ServiceNullable<string?> F { get; set; }

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		protected override JsonServiceSerializer JsonSerializer => SystemTextJsonServiceSerializer.Instance;

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MiscRequestDto? other)
		{
			return other != null &&
				Q == other.Q &&
				F == other.F;
		}
	}
}
