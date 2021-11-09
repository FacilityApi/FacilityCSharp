// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Facility.Core;
using Newtonsoft.Json;

namespace Facility.ConformanceApi
{
	/// <summary>
	/// One of three answers.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	[JsonConverter(typeof(AnswerJsonConverter))]
	public partial struct Answer : IEquatable<Answer>
	{
		/// <summary>
		/// Affirmative.
		/// </summary>
		public static readonly Answer Yes = new Answer(Strings.Yes);

		/// <summary>
		/// Negative.
		/// </summary>
		public static readonly Answer No = new Answer(Strings.No);

		/// <summary>
		/// Unknown.
		/// </summary>
		public static readonly Answer Maybe = new Answer(Strings.Maybe);

		/// <summary>
		/// Creates an instance.
		/// </summary>
		public Answer(string value) => m_value = value;

		/// <summary>
		/// Converts the instance to a string.
		/// </summary>
		public override string ToString() => m_value != null && s_valueCache.TryGetValue(m_value, out var cachedValue) ? cachedValue : m_value ?? "";

		/// <summary>
		/// Checks for equality.
		/// </summary>
		public bool Equals(Answer other) => StringComparer.OrdinalIgnoreCase.Equals(m_value ?? "", other.m_value ?? "");

		/// <summary>
		/// Checks for equality.
		/// </summary>
		public override bool Equals(object? obj) => obj is Answer && Equals((Answer) obj);

		/// <summary>
		/// Gets the hash code.
		/// </summary>
		public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(m_value ?? "");

		/// <summary>
		/// Checks for equality.
		/// </summary>
		public static bool operator ==(Answer left, Answer right) => left.Equals(right);

		/// <summary>
		/// Checks for inequality.
		/// </summary>
		public static bool operator !=(Answer left, Answer right) => !left.Equals(right);

		/// <summary>
		/// Returns true if the instance is equal to one of the defined values.
		/// </summary>
		public bool IsDefined() => m_value != null && s_valueCache.ContainsKey(m_value);

		/// <summary>
		/// Returns all of the defined values.
		/// </summary>
		public static IReadOnlyList<Answer> GetValues() => s_values;

		/// <summary>
		/// Provides string constants for defined values.
		/// </summary>
		public static class Strings
		{
			/// <summary>
			/// Affirmative.
			/// </summary>
			public const string Yes = "yes";

			/// <summary>
			/// Negative.
			/// </summary>
			public const string No = "no";

			/// <summary>
			/// Unknown.
			/// </summary>
			public const string Maybe = "maybe";
		}

		/// <summary>
		/// Used for JSON serialization.
		/// </summary>
		public sealed class AnswerJsonConverter : ServiceEnumJsonConverter<Answer>
		{
			/// <summary>
			/// Creates the value from a string.
			/// </summary>
			protected override Answer CreateCore(string value) => new Answer(value);
		}

		private static readonly ReadOnlyCollection<Answer> s_values = new ReadOnlyCollection<Answer>(
			new[]
			{
				Yes,
				No,
				Maybe,
			});

		private static readonly IReadOnlyDictionary<string, string> s_valueCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ Strings.Yes, Strings.Yes },
			{ Strings.No, Strings.No },
			{ Strings.Maybe, Strings.Maybe },
		};

		private readonly string m_value;
	}
}