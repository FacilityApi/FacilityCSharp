using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

/// <summary>
/// Helper methods for service data.
/// </summary>
public static class ServiceDataUtility
{
	/// <summary>
	/// True if the DTOs are equivalent.
	/// </summary>
	public static bool AreEquivalentDtos(ServiceDto? first, ServiceDto? second) => first == second || first != null && first.IsEquivalentTo(second);

	/// <summary>
	/// True if the results are equivalent.
	/// </summary>
	public static bool AreEquivalentResults(ServiceResult? first, ServiceResult? second) => first == second || first != null && first.IsEquivalentTo(second);

	/// <summary>
	/// True if the objects are equivalent.
	/// </summary>
	public static bool AreEquivalentObjects(JObject? first, JObject? second) => JToken.DeepEquals(first, second);

	/// <summary>
	/// True if the objects are equivalent.
	/// </summary>
	public static bool AreEquivalentObjects(ServiceObject? first, ServiceObject? second) => first == second || first != null && first.IsEquivalentTo(second);

	/// <summary>
	/// True if the bytes are equivalent.
	/// </summary>
	public static bool AreEquivalentBytes(byte[]? first, byte[]? second)
	{
		if (first == null)
			return second == null;
		if (second == null)
			return false;
		if (first.Length != second.Length)
			return false;
		for (var i = 0; i < first.Length; i++)
		{
			if (first[i] != second[i])
				return false;
		}
		return true;
	}

	/// <summary>
	/// True if the arrays are equivalent.
	/// </summary>
	public static bool AreEquivalentArrays<T>(IReadOnlyList<T>? first, IReadOnlyList<T>? second, Func<T, T, bool> areEquivalent)
	{
		if (ReferenceEquals(first, second))
			return true;
		if (first == null || second == null || first.Count != second.Count)
			return false;
		for (var i = 0; i < first.Count; i++)
		{
			if (!areEquivalent(first[i], second[i]))
				return false;
		}
		return true;
	}

	/// <summary>
	/// True if the maps are equivalent.
	/// </summary>
	public static bool AreEquivalentMaps<T>(IReadOnlyDictionary<string, T>? first, IReadOnlyDictionary<string, T>? second, Func<T, T, bool> areEquivalent)
	{
		if (ReferenceEquals(first, second))
			return true;
		if (first == null || second == null || first.Count != second.Count)
			return false;
		foreach (var pair in first)
		{
			if (!second.TryGetValue(pair.Key, out var value) || !areEquivalent(pair.Value, value))
				return false;
		}
		return true;
	}

	/// <summary>
	/// True if the field values are equal.
	/// </summary>
	public static bool AreEquivalentFieldValues<T>(T x, T y) => EquivalenceComparerCache<T>.Instance.Equals(x, y);

	/// <summary>
	/// Validates the field value.
	/// </summary>
	public static bool ValidateFieldValue<T>(T value, out string? errorMessage)
	{
		errorMessage = ValidatorCache<T>.Instance.GetErrorMessage(value, null);
		return errorMessage is null;
	}

	/// <summary>
	/// Validates the field value.
	/// </summary>
	public static bool ValidateFieldValue<T>(T value, string fieldName, out string? errorMessage)
	{
		errorMessage = ValidatorCache<T>.Instance.GetErrorMessage(value, fieldName);
		return errorMessage is null;
	}

	/// <summary>
	/// Clones the data element.
	/// </summary>
	[Obsolete("Use the overload with the serializer.")]
	public static T Clone<T>(T value) => Clone(value, JsonServiceSerializer.Legacy);

	/// <summary>
	/// Clones the data element.
	/// </summary>
	[return: NotNullIfNotNull("value")]
	public static T Clone<T>(T value, JsonServiceSerializer serializer) => value is null ? default! : serializer.FromJson<T>(serializer.ToJson(value))!;

	/// <summary>
	/// Attempts to parse a Boolean.
	/// </summary>
	public static bool? TryParseBoolean(string? text) => bool.TryParse(text, out var value) ? value : default(bool?);

	/// <summary>
	/// Attempts to parse an Int32.
	/// </summary>
	public static int? TryParseInt32(string? text) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : default(int?);

	/// <summary>
	/// Attempts to parse an Int64.
	/// </summary>
	public static long? TryParseInt64(string? text) => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : default(long?);

	/// <summary>
	/// Attempts to parse a Double.
	/// </summary>
	public static double? TryParseDouble(string? text) => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : default(double?);

	/// <summary>
	/// Attempts to parse a Decimal.
	/// </summary>
	public static decimal? TryParseDecimal(string? text) => decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : default(decimal?);

	/// <summary>
	/// Returns the required field error message.
	/// </summary>
	public static string GetRequiredFieldErrorMessage(string fieldName) => $"'{fieldName}' is required.";

	/// <summary>
	/// Returns the invalid field error message prefix.
	/// </summary>
	public static string GetInvalidFieldErrorMessage(string fieldName, string errorMessage) => $"'{fieldName}' is invalid: {errorMessage}";

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
				return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ServiceDtoEquivalenceComparer<>).MakeGenericType(type))!;

			if (typeof(ServiceResult).GetTypeInfo().IsAssignableFrom(typeInfo))
				return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ServiceResultEquivalenceComparer<>).MakeGenericType(type))!;

			if (type == typeof(ServiceObject))
				return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ServiceObjectEquivalenceComparer))!;

			if (type == typeof(JObject))
				return (IEqualityComparer<T>) Activator.CreateInstance(typeof(JObjectEquivalenceComparer))!;

			var interfaces = new[] { type }.Concat(typeInfo.ImplementedInterfaces).ToList();

			var mapInterface = interfaces.FirstOrDefault(x => x.IsConstructedGenericType &&
				x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) && x.GetTypeInfo().GenericTypeArguments[0] == typeof(string));
			if (mapInterface != null)
			{
				var genericTypeArguments = mapInterface.GetTypeInfo().GenericTypeArguments;
				var valueType = genericTypeArguments[1];
				return (IEqualityComparer<T>) Activator.CreateInstance(typeof(MapEquivalenceComparer<,>).MakeGenericType(type, valueType))!;
			}

			var arrayInterface = interfaces.FirstOrDefault(x => x.IsConstructedGenericType &&
				x.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
			if (arrayInterface != null)
			{
				var itemType = arrayInterface.GetTypeInfo().GenericTypeArguments[0];
				return (IEqualityComparer<T>) Activator.CreateInstance(typeof(ArrayEquivalenceComparer<,>).MakeGenericType(type, itemType))!;
			}

			throw new InvalidOperationException($"Type not supported for equivalence: {typeof(T)}");
		}
	}

	private abstract class NoHashCodeEqualityComparer<T> : EqualityComparer<T>
	{
		public sealed override int GetHashCode(T obj) => throw new NotImplementedException();
	}

	private sealed class ServiceDtoEquivalenceComparer<T> : NoHashCodeEqualityComparer<T>
		where T : ServiceDto
	{
		public override bool Equals(T? x, T? y) => AreEquivalentDtos(x, y);
	}

	private sealed class ServiceResultEquivalenceComparer<T> : NoHashCodeEqualityComparer<T>
		where T : ServiceResult
	{
		public override bool Equals(T? x, T? y) => AreEquivalentResults(x, y);
	}

	private sealed class ServiceObjectEquivalenceComparer : NoHashCodeEqualityComparer<ServiceObject>
	{
		public override bool Equals(ServiceObject? x, ServiceObject? y) => AreEquivalentObjects(x, y);
	}

	private sealed class JObjectEquivalenceComparer : NoHashCodeEqualityComparer<JObject>
	{
		public override bool Equals(JObject? x, JObject? y) => AreEquivalentObjects(x, y);
	}

	private sealed class ArrayEquivalenceComparer<T, TItem> : NoHashCodeEqualityComparer<T>
		where T : IReadOnlyList<TItem>
	{
		public override bool Equals(T? x, T? y) => AreEquivalentArrays(x, y, EquivalenceComparerCache<TItem>.Instance.Equals);
	}

	private sealed class MapEquivalenceComparer<T, TValue> : NoHashCodeEqualityComparer<T>
		where T : IReadOnlyDictionary<string, TValue>
	{
		public override bool Equals(T? x, T? y) => AreEquivalentMaps(x, y, EquivalenceComparerCache<TValue>.Instance.Equals);
	}

	private interface IValidator<in T>
	{
		string? GetErrorMessage(T value, string? fieldName);
	}

	private static class ValidatorCache<T>
	{
		public static readonly IValidator<T> Instance = CreateInstance();

		private static IValidator<T> CreateInstance()
		{
			var type = typeof(T);
			var typeInfo = type.GetTypeInfo();

			if (Nullable.GetUnderlyingType(type) != null || typeof(IEquatable<T>).GetTypeInfo().IsAssignableFrom(typeInfo))
				return new AlwaysValidValidator<T>();

			if (typeof(ServiceDto).GetTypeInfo().IsAssignableFrom(typeInfo))
				return (IValidator<T>) Activator.CreateInstance(typeof(ServiceDtoValidator<>).MakeGenericType(type))!;

			if (typeof(ServiceResult).GetTypeInfo().IsAssignableFrom(typeInfo))
				return (IValidator<T>) Activator.CreateInstance(typeof(ServiceResultValidator<>).MakeGenericType(type))!;

			if (type == typeof(JObject) || type == typeof(byte[]))
				return new AlwaysValidValidator<T>();

			var interfaces = new[] { type }.Concat(typeInfo.ImplementedInterfaces).ToList();

			var mapInterface = interfaces.FirstOrDefault(x => x.IsConstructedGenericType &&
				x.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) && x.GetTypeInfo().GenericTypeArguments[0] == typeof(string));
			if (mapInterface != null)
			{
				var genericTypeArguments = mapInterface.GetTypeInfo().GenericTypeArguments;
				var valueType = genericTypeArguments[1];
				return (IValidator<T>) Activator.CreateInstance(typeof(MapValidator<,>).MakeGenericType(type, valueType))!;
			}

			var arrayInterface = interfaces.FirstOrDefault(x => x.IsConstructedGenericType &&
				x.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
			if (arrayInterface != null)
			{
				var itemType = arrayInterface.GetTypeInfo().GenericTypeArguments[0];
				return (IValidator<T>) Activator.CreateInstance(typeof(ArrayValidator<,>).MakeGenericType(type, itemType))!;
			}

			throw new InvalidOperationException($"Type not supported for validation: {typeof(T)}");
		}
	}

	private sealed class AlwaysValidValidator<T> : IValidator<T>
	{
		public string? GetErrorMessage(T value, string? fieldName) => null;
	}

	private sealed class ServiceDtoValidator<T> : IValidator<T>
		where T : ServiceDto
	{
		public string? GetErrorMessage(T? value, string? fieldName)
		{
			if (value is null || value.Validate(out var errorMessage))
				return null;

			return fieldName is null ? errorMessage : GetInvalidFieldErrorMessage(fieldName, errorMessage!);
		}
	}

	private sealed class ServiceResultValidator<T> : IValidator<T>
		where T : ServiceResult
	{
		public string? GetErrorMessage(T? value, string? fieldName)
		{
			if (value is null || value.Validate(out var errorMessage))
				return null;

			return fieldName is null ? errorMessage : GetInvalidFieldErrorMessage(fieldName, errorMessage!);
		}
	}

	private sealed class ArrayValidator<T, TItem> : IValidator<T>
		where T : class, IReadOnlyList<TItem>
	{
		public string? GetErrorMessage(T? value, string? fieldName)
		{
			if (value is null)
				return null;

			var itemValidator = ValidatorCache<TItem>.Instance;
			for (var index = 0; index < value.Count; index++)
			{
				var item = value[index];
				if (itemValidator.GetErrorMessage(item, fieldName is null ? null : $"{fieldName}[{index}]") is string errorMessage)
					return errorMessage;
			}

			return null;
		}
	}

	private sealed class MapValidator<T, TValue> : IValidator<T>
		where T : class, IReadOnlyDictionary<string, TValue>
	{
		public string? GetErrorMessage(T? value, string? fieldName)
		{
			if (value is null)
				return null;

			var itemValidator = ValidatorCache<TValue>.Instance;
			foreach (var keyValuePair in value)
			{
				if (itemValidator.GetErrorMessage(keyValuePair.Value, fieldName is null ? null : $"{fieldName}.{keyValuePair.Key}") is string errorMessage)
					return errorMessage;
			}

			return null;
		}
	}
}
