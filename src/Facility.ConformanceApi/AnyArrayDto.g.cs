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
	public sealed partial class AnyArrayDto : ServiceDto<AnyArrayDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public AnyArrayDto()
		{
		}

		[ProtoMember(1)]
		public IReadOnlyList<string>? String { get; set; }

		[ProtoMember(2)]
		public IReadOnlyList<bool>? Boolean { get; set; }

		[ProtoMember(3)]
		public IReadOnlyList<double>? Double { get; set; }

		[ProtoMember(4)]
		public IReadOnlyList<int>? Int32 { get; set; }

		[ProtoMember(5)]
		public IReadOnlyList<long>? Int64 { get; set; }

		[ProtoMember(6)]
		public IReadOnlyList<decimal>? Decimal { get; set; }

		[ProtoMember(7)]
		public IReadOnlyList<byte[]>? Bytes { get; set; }

		[ProtoMember(8)]
		public IReadOnlyList<ServiceObject>? Object { get; set; }

		[ProtoMember(9)]
		public IReadOnlyList<ServiceErrorDto>? Error { get; set; }

		[ProtoMember(10)]
		public IReadOnlyList<AnyDto>? Data { get; set; }

		[ProtoMember(11)]
		public IReadOnlyList<Answer>? Enum { get; set; }

		[ProtoMember(12)]
		public IReadOnlyList<IReadOnlyList<int>>? Array { get; set; }

		[ProtoMember(13)]
		public IReadOnlyList<IReadOnlyDictionary<string, int>>? Map { get; set; }

		[ProtoMember(14)]
		public IReadOnlyList<ServiceResult<int>>? Result { get; set; }

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(AnyArrayDto? other)
		{
			return other != null &&
				ServiceDataUtility.AreEquivalentFieldValues(String, other.String) &&
				ServiceDataUtility.AreEquivalentFieldValues(Boolean, other.Boolean) &&
				ServiceDataUtility.AreEquivalentFieldValues(Double, other.Double) &&
				ServiceDataUtility.AreEquivalentFieldValues(Int32, other.Int32) &&
				ServiceDataUtility.AreEquivalentFieldValues(Int64, other.Int64) &&
				ServiceDataUtility.AreEquivalentFieldValues(Decimal, other.Decimal) &&
				ServiceDataUtility.AreEquivalentFieldValues(Bytes, other.Bytes) &&
				ServiceDataUtility.AreEquivalentFieldValues(Object, other.Object) &&
				ServiceDataUtility.AreEquivalentFieldValues(Error, other.Error) &&
				ServiceDataUtility.AreEquivalentFieldValues(Data, other.Data) &&
				ServiceDataUtility.AreEquivalentFieldValues(Enum, other.Enum) &&
				ServiceDataUtility.AreEquivalentFieldValues(Array, other.Array) &&
				ServiceDataUtility.AreEquivalentFieldValues(Map, other.Map) &&
				ServiceDataUtility.AreEquivalentFieldValues(Result, other.Result);
		}
	}
}
