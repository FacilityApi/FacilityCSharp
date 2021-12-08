// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Facility.Core;

namespace Facility.ConformanceApi
{
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[MessagePack.MessagePackObject]
	public sealed partial class AnyMapDto : ServiceDto<AnyMapDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public AnyMapDto()
		{
		}

		[MessagePack.Key("string")]
		public IReadOnlyDictionary<string, string>? String { get; set; }

		[MessagePack.Key("boolean")]
		public IReadOnlyDictionary<string, bool>? Boolean { get; set; }

		[MessagePack.Key("double")]
		public IReadOnlyDictionary<string, double>? Double { get; set; }

		[MessagePack.Key("int32")]
		public IReadOnlyDictionary<string, int>? Int32 { get; set; }

		[MessagePack.Key("int64")]
		public IReadOnlyDictionary<string, long>? Int64 { get; set; }

		[MessagePack.Key("decimal")]
		public IReadOnlyDictionary<string, decimal>? Decimal { get; set; }

		[MessagePack.Key("bytes")]
		public IReadOnlyDictionary<string, byte[]>? Bytes { get; set; }

		[MessagePack.Key("object")]
		public IReadOnlyDictionary<string, ServiceObject>? Object { get; set; }

		[MessagePack.Key("error")]
		public IReadOnlyDictionary<string, ServiceErrorDto>? Error { get; set; }

		[MessagePack.Key("data")]
		public IReadOnlyDictionary<string, AnyDto>? Data { get; set; }

		[MessagePack.Key("enum")]
		public IReadOnlyDictionary<string, Answer>? Enum { get; set; }

		[MessagePack.Key("array")]
		public IReadOnlyDictionary<string, IReadOnlyList<int>>? Array { get; set; }

		[MessagePack.Key("map")]
		public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>? Map { get; set; }

		[MessagePack.Key("result")]
		public IReadOnlyDictionary<string, ServiceResult<int>>? Result { get; set; }

		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => SystemTextJsonServiceSerializer.Instance.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(AnyMapDto? other)
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
