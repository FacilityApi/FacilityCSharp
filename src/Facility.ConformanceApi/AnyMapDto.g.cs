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
	public sealed partial class AnyMapDto : ServiceDto<AnyMapDto>
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public AnyMapDto()
		{
		}

		public IReadOnlyDictionary<string, string>? String { get; set; }

		public IReadOnlyDictionary<string, bool>? Boolean { get; set; }

		public IReadOnlyDictionary<string, double>? Double { get; set; }

		public IReadOnlyDictionary<string, int>? Int32 { get; set; }

		public IReadOnlyDictionary<string, long>? Int64 { get; set; }

		public IReadOnlyDictionary<string, decimal>? Decimal { get; set; }

		public IReadOnlyDictionary<string, byte[]>? Bytes { get; set; }

		public IReadOnlyDictionary<string, ServiceObject>? Object { get; set; }

		public IReadOnlyDictionary<string, ServiceErrorDto>? Error { get; set; }

		public IReadOnlyDictionary<string, AnyDto>? Data { get; set; }

		public IReadOnlyDictionary<string, Answer>? Enum { get; set; }

		public IReadOnlyDictionary<string, IReadOnlyList<int>>? Array { get; set; }

		public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>? Map { get; set; }

		public IReadOnlyDictionary<string, ServiceResult<int>>? Result { get; set; }

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
