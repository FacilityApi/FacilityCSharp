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
	/// Request for Mixed.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class MixedRequestDto : ServiceDto<MixedRequestDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public MixedRequestDto()
		{
		}

		public string Path { get; set; }

		public string Query { get; set; }

		public string Header { get; set; }

		public string Normal { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(MixedRequestDto other)
		{
			return other != null &&
				Path == other.Path &&
				Query == other.Query &&
				Header == other.Header &&
				Normal == other.Normal;
		}
	}
}
