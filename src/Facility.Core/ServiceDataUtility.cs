using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Facility.Core
{
	/// <summary>
	/// Helper methods for service data.
	/// </summary>
	public static class ServiceDataUtility
	{
		/// <summary>
		/// True if the data elements are equal.
		/// </summary>
		public static bool AreEquivalent<T>(T x, T y)
		{
			return GetEquivalenceComparer<T>().Equals(x, y);
		}

		/// <summary>
		/// Gets an IEqualityComparer for data elements.
		/// </summary>
		public static IEqualityComparer<T> GetEquivalenceComparer<T>()
		{
			return EquivalenceComparerCache<T>.Instance;
		}

		/// <summary>
		/// Clones the data element.
		/// </summary>
		public static T Clone<T>(T value)
		{
			return value == null ? default(T) : ServiceJsonUtility.FromJson<T>(ServiceJsonUtility.ToJson(value));
		}

		private static bool AreDictionariesEquivalent<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> left, IReadOnlyDictionary<TKey, TValue> right)
		{
			if (left == right)
				return true;

			if (left == null || right == null || left.Count != right.Count)
				return false;

			foreach (var pair in left)
			{
				TValue value;
				if (!right.TryGetValue(pair.Key, out value) || !AreEquivalent(pair.Value, value))
					return false;
			}

			return true;
		}

		private static class EquivalenceComparerCache<T>
		{
			public static readonly IEqualityComparer<T> Instance = CreateInstance();

			private static IEqualityComparer<T> CreateInstance()
			{
				var type = typeof(T);
				var typeInfo = type.GetTypeInfo();

				if (typeof(IEquatable<T>).GetTypeInfo().IsAssignableFrom(typeInfo))
					return EqualityComparer<T>.Default;

				if (typeof(IServiceData<T>).GetTypeInfo().IsAssignableFrom(typeInfo))
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ServiceDataEquivalenceComparer<>).MakeGenericType(type));

				var dictionaryInterfaces = typeInfo.ImplementedInterfaces
					.Where(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)).ToList();
				if (dictionaryInterfaces.Count == 1)
				{
					var genericTypeArguments = dictionaryInterfaces[0].GetTypeInfo().GenericTypeArguments;
					var keyType = genericTypeArguments[0];
					var valueType = genericTypeArguments[1];
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(DictionaryEquivalenceComparer<,,>).MakeGenericType(type, keyType, valueType));
				}

				var enumerableInterfaces = typeInfo.ImplementedInterfaces
					.Where(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).ToList();
				if (enumerableInterfaces.Count == 1)
				{
					var itemType = enumerableInterfaces[0].GetTypeInfo().GenericTypeArguments[0];
					return (IEqualityComparer<T>) Activator.CreateInstance(typeof(EnumerableEquivalenceComparer<,>).MakeGenericType(type, itemType));
				}

				return new ObjectEquivalenceComparer<T>();
			}
		}

		private abstract class NoHashCodeEqualityComparer<T> : EqualityComparer<T>
		{
			public sealed override int GetHashCode(T obj)
			{
				throw new NotImplementedException();
			}
		}

		private sealed class EnumerableEquivalenceComparer<T, TItem> : NoHashCodeEqualityComparer<T>
			where T : IEnumerable<TItem>
		{
			public override bool Equals(T x, T y)
			{
				return x == null ? y == null : x.SequenceEqual(y, GetEquivalenceComparer<TItem>());
			}
		}

		private sealed class DictionaryEquivalenceComparer<T, TKey, TValue> : NoHashCodeEqualityComparer<T>
			where T : IReadOnlyDictionary<TKey, TValue>
		{
			public override bool Equals(T x, T y)
			{
				return AreDictionariesEquivalent(x, y);
			}
		}

		private sealed class ServiceDataEquivalenceComparer<T> : NoHashCodeEqualityComparer<T>
			where T : IServiceData<T>
		{
			public override bool Equals(T x, T y)
			{
				return x == null ? y == null : x.IsEquivalentTo(y);
			}
		}

		private sealed class ObjectEquivalenceComparer<T> : NoHashCodeEqualityComparer<T>
		{
			public override bool Equals(T x, T y)
			{
				if (x == null)
					return y == null;
				if (y == null)
					return false;

				var leftAsString = x as string;
				if (leftAsString != null)
					return leftAsString == y as string;

				var xAsData = x as IServiceData;
				if (xAsData != null)
				{
					var yAsData = y as IServiceData;
					return yAsData != null && xAsData.IsEquivalentTo(yAsData);
				}

				var xAsEnumerable = x as IEnumerable;
				if (xAsEnumerable != null)
				{
					var yAsEnumerable = y as IEnumerable;
					return yAsEnumerable != null && xAsEnumerable.Cast<object>().SequenceEqual(yAsEnumerable.Cast<object>(), GetEquivalenceComparer<object>());
				}

				return Default.Equals(x, y);
			}
		}
	}
}
