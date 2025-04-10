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
	public sealed partial class AnyDto : ServiceDto<AnyDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public AnyDto()
		{
		}

		[MessagePack.Key("string")]
		public string? String { get; set; }

		[MessagePack.Key("boolean")]
		public bool? Boolean { get; set; }

		[MessagePack.Key("float")]
		public float? Float { get; set; }

		[MessagePack.Key("double")]
		public double? Double { get; set; }

		[MessagePack.Key("int32")]
		public int? Int32 { get; set; }

		[MessagePack.Key("int64")]
		public long? Int64 { get; set; }

		[MessagePack.Key("decimal")]
		public decimal? Decimal { get; set; }

		[MessagePack.Key("datetime")]
		public DateTime? Datetime { get; set; }

		[MessagePack.Key("bytes")]
		public byte[]? Bytes { get; set; }

		[MessagePack.Key("object")]
		public ServiceObject? Object { get; set; }

		[MessagePack.Key("error")]
		public ServiceErrorDto? Error { get; set; }

		[MessagePack.Key("data")]
		public AnyDto? Data { get; set; }

		[MessagePack.Key("enum")]
		public Answer? Enum { get; set; }

		[MessagePack.Key("array")]
		public AnyArrayDto? Array { get; set; }

		[MessagePack.Key("map")]
		public AnyMapDto? Map { get; set; }

		[MessagePack.Key("result")]
		public AnyResultDto? Result { get; set; }

		[MessagePack.Key("nullable")]
		public AnyNullableDto? Nullable { get; set; }

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		protected override JsonServiceSerializer JsonSerializer => SystemTextJsonServiceSerializer.Instance;

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public override bool IsEquivalentTo(AnyDto? other)
		{
			return other != null &&
				String == other.String &&
				Boolean == other.Boolean &&
				Float.Equals(other.Float) &&
				Double.Equals(other.Double) &&
				Int32 == other.Int32 &&
				Int64 == other.Int64 &&
				Decimal == other.Decimal &&
				ServiceDataUtility.AreEquivalentDateTimes(Datetime, other.Datetime) &&
				ServiceDataUtility.AreEquivalentBytes(Bytes, other.Bytes) &&
				ServiceDataUtility.AreEquivalentObjects(Object, other.Object) &&
				ServiceDataUtility.AreEquivalentDtos(Error, other.Error) &&
				ServiceDataUtility.AreEquivalentDtos(Data, other.Data) &&
				Enum == other.Enum &&
				ServiceDataUtility.AreEquivalentDtos(Array, other.Array) &&
				ServiceDataUtility.AreEquivalentDtos(Map, other.Map) &&
				ServiceDataUtility.AreEquivalentDtos(Result, other.Result) &&
				ServiceDataUtility.AreEquivalentDtos(Nullable, other.Nullable);
		}
	}
}
