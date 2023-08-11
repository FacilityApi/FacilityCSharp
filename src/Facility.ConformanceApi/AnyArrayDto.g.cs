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
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class AnyArrayDto : ServiceDto<AnyArrayDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public AnyArrayDto()
		{
		}

		[MessagePack.Key("string")]
		public IReadOnlyList<string>? String { get; set; }

		[MessagePack.Key("boolean")]
		public IReadOnlyList<bool>? Boolean { get; set; }

		[MessagePack.Key("double")]
		public IReadOnlyList<double>? Double { get; set; }

		[MessagePack.Key("int32")]
		public IReadOnlyList<int>? Int32 { get; set; }

		[MessagePack.Key("int64")]
		public IReadOnlyList<long>? Int64 { get; set; }

		[MessagePack.Key("decimal")]
		public IReadOnlyList<decimal>? Decimal { get; set; }

		[MessagePack.Key("datetime")]
		public IReadOnlyList<DateTime>? Datetime { get; set; }

		[MessagePack.Key("bytes")]
		public IReadOnlyList<byte[]>? Bytes { get; set; }

		[MessagePack.Key("object")]
		public IReadOnlyList<ServiceObject>? Object { get; set; }

		[MessagePack.Key("error")]
		public IReadOnlyList<ServiceErrorDto>? Error { get; set; }

		[MessagePack.Key("data")]
		public IReadOnlyList<AnyDto>? Data { get; set; }

		[MessagePack.Key("enum")]
		public IReadOnlyList<Answer>? Enum { get; set; }

		[MessagePack.Key("array")]
		public IReadOnlyList<IReadOnlyList<int>>? Array { get; set; }

		[MessagePack.Key("map")]
		public IReadOnlyList<IReadOnlyDictionary<string, int>>? Map { get; set; }

		[MessagePack.Key("result")]
		public IReadOnlyList<ServiceResult<int>>? Result { get; set; }

		[MessagePack.Key("nullable")]
		public IReadOnlyList<int?>? Nullable { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

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
				ServiceDataUtility.AreEquivalentFieldValues(Datetime, other.Datetime) &&
				ServiceDataUtility.AreEquivalentFieldValues(Bytes, other.Bytes) &&
				ServiceDataUtility.AreEquivalentFieldValues(Object, other.Object) &&
				ServiceDataUtility.AreEquivalentFieldValues(Error, other.Error) &&
				ServiceDataUtility.AreEquivalentFieldValues(Data, other.Data) &&
				ServiceDataUtility.AreEquivalentFieldValues(Enum, other.Enum) &&
				ServiceDataUtility.AreEquivalentFieldValues(Array, other.Array) &&
				ServiceDataUtility.AreEquivalentFieldValues(Map, other.Map) &&
				ServiceDataUtility.AreEquivalentFieldValues(Result, other.Result) &&
				ServiceDataUtility.AreEquivalentFieldValues(Nullable, other.Nullable);
		}
	}
}
