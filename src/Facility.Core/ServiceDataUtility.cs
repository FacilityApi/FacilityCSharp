using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Facility.Core
{
	/// <summary>
	/// Helper methods for service data.
	/// </summary>
	public static class ServiceDataUtility
	{
		/// <summary>
		/// True if the DTOs are equivalent.
		/// </summary>
		public static bool AreEquivalentDtos(ServiceDto first, ServiceDto second)
		{
			return first == second || first != null && first.IsEquivalentTo(second);
		}

		/// <summary>
		/// True if the results are equivalent.
		/// </summary>
		public static bool AreEquivalentResults(ServiceResult first, ServiceResult second)
		{
			return first == second || first != null && first.IsEquivalentTo(second);
		}

		/// <summary>
		/// True if the objects are equivalent.
		/// </summary>
		public static bool AreEquivalentObjects(JObject first, JObject second)
		{
			return JToken.DeepEquals(first, second);
		}

		/// <summary>
		/// True if the bytes are equivalent.
		/// </summary>
		public static bool AreEquivalentBytes(byte[] first, byte[] second)
		{
			if (first == null)
				return second == null;
			if (second == null)
				return false;
			if (first.Length != second.Length)
				return false;
			for (int i = 0; i < first.Length; i++)
			{
				if (first[i] != second[i])
					return false;
			}
			return true;
		}

		/// <summary>
		/// True if the arrays are equivalent.
		/// </summary>
		public static bool AreEquivalentArrays<T>(IReadOnlyList<T> first, IReadOnlyList<T> second)
		{
			return AreEquivalentArrays(first, second, EqualityComparer<T>.Default.Equals);
		}

		/// <summary>
		/// True if the arrays are equivalent.
		/// </summary>
		public static bool AreEquivalentArrays<T>(IReadOnlyList<T> first, IReadOnlyList<T> second, Func<T, T, bool> areEquivalent)
		{
			if (ReferenceEquals(first, second))
				return true;
			if (first == null || second == null || first.Count != second.Count)
				return false;
			for (int i = 0; i < first.Count; i++)
			{
				if (!areEquivalent(first[i], second[i]))
					return false;
			}
			return true;
		}

		/// <summary>
		/// True if the maps are equivalent.
		/// </summary>
		public static bool AreEquivalentMaps<T>(IReadOnlyDictionary<string, T> first, IReadOnlyDictionary<string, T> second)
		{
			return AreEquivalentMaps(first, second, EqualityComparer<T>.Default.Equals);
		}

		/// <summary>
		/// True if the maps are equivalent.
		/// </summary>
		public static bool AreEquivalentMaps<T>(IReadOnlyDictionary<string, T> first, IReadOnlyDictionary<string, T> second, Func<T, T, bool> areEquivalent)
		{
			if (ReferenceEquals(first, second))
				return true;
			if (first == null || second == null || first.Count != second.Count)
				return false;
			foreach (var pair in first)
			{
				T value;
				if (!second.TryGetValue(pair.Key, out value) || !areEquivalent(pair.Value, value))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Clones the data element.
		/// </summary>
		public static T Clone<T>(T value)
		{
			return value == null ? default(T) : ServiceJsonUtility.FromJson<T>(ServiceJsonUtility.ToJson(value));
		}
	}
}
