using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Facility.Core.IO;
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
		/// True if the field values are equal.
		/// </summary>
		public static bool AreEquivalentFieldValues<T>(T x, T y)
		{
			return EquivalenceComparerCache<T>.Instance.Equals(x, y);
		}

		/// <summary>
		/// Clones the data element.
		/// </summary>
		public static T Clone<T>(T value)
		{
			return value == null ? default(T) : ServiceJsonUtility.FromJson<T>(ServiceJsonUtility.ToJson(value));
		}

		/// <summary>
		/// Attempts to parse a Boolean.
		/// </summary>
		public static bool? TryParseBoolean(string text)
		{
			bool value;
			return bool.TryParse(text, out value) ? value : default(bool?);
		}

		/// <summary>
		/// Attempts to parse an Int32.
		/// </summary>
		public static int? TryParseInt32(string text)
		{
			int value;
			return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : default(int?);
		}

		/// <summary>
		/// Attempts to parse an Int64.
		/// </summary>
		public static long? TryParseInt64(string text)
		{
			long value;
			return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : default(long?);
		}

		/// <summary>
		/// Attempts to parse a Double.
		/// </summary>
		public static double? TryParseDouble(string text)
		{
			double value;
			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : default(double?);
		}

		internal static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

		private static class EquivalenceComparerCache<T>
		{
			public static readonly IEqualityComparer<T> Instance = CreateInstance();

			private static IEqualityComparer<T> CreateInstance()
			{
				var type = typeof(T);
				var typeInfo = type.GetTypeInfo();

				if (Nullable.GetUnderlyingType(type) != null || typeof(IEquatable<T>).GetTypeInfo().IsAssignableFrom(typeInfo))
					return EqualityComparer<T>.Default;

				if (typeof(ServiceDto).GetTypeInfo().IsAssignableFrom(typeInfo))
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ServiceDtoEquivalenceComparer<>).MakeGenericType(type));

				if (typeof(ServiceResult).GetTypeInfo().IsAssignableFrom(typeInfo))
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ServiceResultEquivalenceComparer<>).MakeGenericType(type));

				if (type == typeof(JObject))
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(JObjectEquivalenceComparer));

				var interfaces = new[] { type }.Concat(typeInfo.ImplementedInterfaces).ToList();

				var mapInterface = interfaces.FirstOrDefault(x => x.IsConstructedGenericType &&
					x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) && x.GetTypeInfo().GenericTypeArguments[0] == typeof(string));
				if (mapInterface != null)
				{
					var genericTypeArguments = mapInterface.GetTypeInfo().GenericTypeArguments;
					var valueType = genericTypeArguments[1];
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(MapEquivalenceComparer<,>).MakeGenericType(type, valueType));
				}

				var arrayInterface = interfaces.FirstOrDefault(x => x.IsConstructedGenericType &&
					x.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
				if (arrayInterface != null)
				{
					var itemType = arrayInterface.GetTypeInfo().GenericTypeArguments[0];
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ArrayEquivalenceComparer<,>).MakeGenericType(type, itemType));
				}

				throw new InvalidOperationException($"Type not supported for equivalence: {typeof(T)}");
			}
		}

		private abstract class NoHashCodeEqualityComparer<T> : EqualityComparer<T>
		{
			public sealed override int GetHashCode(T obj)
			{
				throw new NotImplementedException();
			}
		}

		private sealed class ServiceDtoEquivalenceComparer<T> : NoHashCodeEqualityComparer<T>
			where T : ServiceDto
		{
			public override bool Equals(T x, T y)
			{
				return AreEquivalentDtos(x, y);
			}
		}

		private sealed class ServiceResultEquivalenceComparer<T> : NoHashCodeEqualityComparer<T>
			where T : ServiceResult
		{
			public override bool Equals(T x, T y)
			{
				return AreEquivalentResults(x, y);
			}
		}

		private sealed class JObjectEquivalenceComparer : NoHashCodeEqualityComparer<JObject>
		{
			public override bool Equals(JObject x, JObject y)
			{
				return AreEquivalentObjects(x, y);
			}
		}

		private sealed class ArrayEquivalenceComparer<T, TItem> : NoHashCodeEqualityComparer<T>
			where T : IReadOnlyList<TItem>
		{
			public override bool Equals(T x, T y)
			{
				return AreEquivalentArrays(x, y, EquivalenceComparerCache<TItem>.Instance.Equals);
			}
		}

		private sealed class MapEquivalenceComparer<T, TValue> : NoHashCodeEqualityComparer<T>
			where T : IReadOnlyDictionary<string, TValue>
		{
			public override bool Equals(T x, T y)
			{
				return AreEquivalentMaps(x, y, EquivalenceComparerCache<TValue>.Instance.Equals);
			}
		}
	}
}
