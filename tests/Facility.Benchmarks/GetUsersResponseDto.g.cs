// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;

namespace Facility.Benchmarks
{
	/// <summary>
	/// Response for GetUsers.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class GetUsersResponseDto : ServiceDto<GetUsersResponseDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public GetUsersResponseDto()
		{
		}

		[MessagePack.Key("items")]
		public IReadOnlyList<UserDto>? Items { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(GetUsersResponseDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentFieldValues(Items, other.Items);
		}
	}
}