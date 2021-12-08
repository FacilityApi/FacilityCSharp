using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json;

namespace Facility.Core;

/// <summary>
/// A service result success or error.
/// </summary>
[JsonConverter(typeof(ServiceResultJsonConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(ServiceResultSystemTextJsonConverter))]
[MessagePack.MessagePackFormatter(typeof(ServiceResultMessagePackFormatter))]
public class ServiceResult
{
	/// <summary>
	/// Creates a successful result.
	/// </summary>
	public static ServiceResult Success() => new(null);

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	public static ServiceResult<T> Success<T>(T value) => new(value);

	/// <summary>
	/// Creates a failed result.
	/// </summary>
	public static ServiceResultFailure Failure(ServiceErrorDto error) => new(error ?? throw new ArgumentNullException(nameof(error)));

	/// <summary>
	/// True if the result has a value.
	/// </summary>
	public bool IsSuccess => Error == null;

	/// <summary>
	/// True if the result has an error.
	/// </summary>
	public bool IsFailure => Error != null;

	/// <summary>
	/// The error.
	/// </summary>
	public ServiceErrorDto? Error { get; }

	/// <summary>
	/// Throws a ServiceException if the result is an error.
	/// </summary>
	public void Verify()
	{
		if (Error != null)
			throw new ServiceException(Error);
	}

	/// <summary>
	/// Casts to a ServiceResult with a value.
	/// </summary>
	/// <remarks>A failed ServiceResult can be cast to a ServiceResult of any type.
	/// A successful ServiceResult can only be cast to a ServiceResult of another type
	/// if its value can be successfully cast through object.</remarks>
	public ServiceResult<T> Cast<T>()
	{
		if (IsFailure)
			return Failure(Error!);
		else
			return Success((T) InternalValue!);
	}

	/// <summary>
	/// The service result as a failure; null if it is a success.
	/// </summary>
	public ServiceResultFailure? AsFailure() => this as ServiceResultFailure ?? (IsFailure ? Failure(Error!) : null);

	/// <summary>
	/// The service result as a failure; throws if it is a success.
	/// </summary>
	public ServiceResultFailure ToFailure() => AsFailure() ?? throw new InvalidOperationException("Result is not a failure.");

	/// <summary>
	/// Check service results for equivalence.
	/// </summary>
	public bool IsEquivalentTo(ServiceResult? other)
	{
		if (other == null)
			return false;

		if (IsFailure)
			return other.IsFailure && ServiceDataUtility.AreEquivalentDtos(Error, other.Error);

		var valueType = InternalValueType;
		if (valueType == null)
			return other.InternalValueType == null;

		return IsInternalValueEquivalent(other);
	}

	/// <summary>
	/// Validates the server result value.
	/// </summary>
	public bool Validate(out string? errorMessage)
	{
		if (IsFailure)
		{
			errorMessage = null;
			return true;
		}

		return ValidateInternalValue(out errorMessage);
	}

	/// <summary>
	/// Render result as a string.
	/// </summary>
	public override string ToString() => IsSuccess ? "<Success>" : $"<Failure={Error}>";

	/// <summary>
	/// Used by Json.NET to convert <see cref="ServiceResult" />.
	/// </summary>
	[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Legacy.")]
	public sealed class ServiceResultJsonConverter : JsonConverter
	{
		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		public override bool CanConvert(Type objectType) => objectType.GetTypeInfo().IsAssignableFrom(typeof(ServiceResult).GetTypeInfo());

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;

			MatchTokenOrThrow(reader, JsonToken.StartObject);
			ReadOrThrow(reader);

			var valueType = objectType.IsConstructedGenericType ? objectType.GenericTypeArguments[0] : null;
			object? value = null;
			ServiceErrorDto? error = null;

			while (reader.TokenType == JsonToken.PropertyName)
			{
				var propertyName = (string) reader.Value;
				ReadOrThrow(reader);

				if (string.Equals(propertyName, c_valuePropertyName, StringComparison.OrdinalIgnoreCase))
				{
					if (valueType == null)
						throw new JsonSerializationException("ServiceResult does not support 'value'; use ServiceResult<T>.");

					value = serializer.Deserialize(reader, valueType);
				}
				else if (string.Equals(propertyName, c_errorPropertyName, StringComparison.OrdinalIgnoreCase))
				{
					error = serializer.Deserialize<ServiceErrorDto>(reader);
				}

				ReadOrThrow(reader);
			}

			MatchTokenOrThrow(reader, JsonToken.EndObject);

			if (value != null && error != null)
				throw new JsonSerializationException("ServiceResult must not have both 'value' and 'error'.");

			if (valueType == null)
			{
				return error != null ? Failure(error) : Success();
			}
			else if (error != null)
			{
				return (ServiceResult) s_genericCastMethod.MakeGenericMethod(valueType).Invoke(Failure(error), Array.Empty<object>())!;
			}
			else
			{
				if (value == null && valueType.GetTypeInfo().IsValueType)
					value = Activator.CreateInstance(valueType);
				return (ServiceResult) s_genericSuccessMethod.MakeGenericMethod(valueType).Invoke(null, new[] { value })!;
			}
		}

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var serviceResult = (ServiceResult) value;
			var valueType = serviceResult.InternalValueType;

			writer.WriteStartObject();
			if (serviceResult.IsFailure)
			{
				writer.WritePropertyName(c_errorPropertyName);
				serializer.Serialize(writer, serviceResult.Error);
			}
			else if (valueType != null)
			{
				writer.WritePropertyName(c_valuePropertyName);
				serializer.Serialize(writer, serviceResult.InternalValue);
			}
			writer.WriteEndObject();
		}

		private static void ReadOrThrow(JsonReader reader)
		{
			if (!reader.Read())
				throw new JsonSerializationException("ServiceResult JSON ended unexpectedly.");
		}

		private static void MatchTokenOrThrow(JsonReader reader, JsonToken tokenType)
		{
			if (reader.TokenType != tokenType)
				throw new JsonSerializationException($"ServiceResult expected {tokenType} but found {reader.TokenType}.");
		}

		private const string c_valuePropertyName = "value";
		private const string c_errorPropertyName = "error";
		private static readonly MethodInfo s_genericSuccessMethod = typeof(ServiceResult).GetRuntimeMethods().First(x => x.Name == "Success" && x.IsStatic && x.IsGenericMethodDefinition);
		private static readonly MethodInfo s_genericCastMethod = typeof(ServiceResult).GetRuntimeMethods().First(x => x.Name == "Cast" && !x.IsStatic && x.IsGenericMethodDefinition);
	}

	internal ServiceResult(ServiceErrorDto? error)
	{
		Error = error;
	}

	internal virtual Type? InternalValueType => null;

	internal virtual object? InternalValue => throw new InvalidCastException("A successful result without a value cannot be cast.");

	internal virtual bool IsInternalValueEquivalent(ServiceResult result) => false;

	internal virtual bool ValidateInternalValue(out string? errorMessage)
	{
		errorMessage = null;
		return true;
	}
}

/// <summary>
/// A service result value or error.
/// </summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
[System.Text.Json.Serialization.JsonConverter(typeof(ServiceResultSystemTextJsonConverter))]
[MessagePack.MessagePackFormatter(typeof(ServiceResultMessagePackFormatter<>))]
public sealed class ServiceResult<T> : ServiceResult
{
	/// <summary>
	/// Implicitly create a failed result from an error.
	/// </summary>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Used to create results from failures.")]
	public static implicit operator ServiceResult<T>(ServiceResultFailure failure) => new(failure.Error);

	/// <summary>
	/// The value. (Throws a ServiceException on failure.)
	/// </summary>
	public T Value
	{
		get
		{
			Verify();
			return m_value!;
		}
	}

	/// <summary>
	/// The value. (Returns null on failure.)
	/// </summary>
	[SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "By design.")]
	public T? GetValueOrDefault() => m_value;

	/// <summary>
	/// Maps a ServiceResult from one type to another.
	/// </summary>
	/// <remarks>If the result is a success, the function is called on the input value to produce
	/// a successful service result matching the type of the output value. If the result is a failure,
	/// the function is not called, and a failed service result using the output type is returned.</remarks>
	public ServiceResult<TOutput> Map<TOutput>(Func<T, TOutput> func) => IsFailure ? new ServiceResult<TOutput>(Error) : new ServiceResult<TOutput>(func(m_value!));

	/// <summary>
	/// Maps a ServiceResult from one type to another.
	/// </summary>
	/// <remarks>If the result is a success, the function is called on the input value to produce
	/// a service result matching the type of the output value. If the result is a failure,
	/// the function is not called, and a failed service result using the output type is returned.</remarks>
	public ServiceResult<TOutput> Map<TOutput>(Func<T, ServiceResult<TOutput>> func) => IsFailure ? new ServiceResult<TOutput>(Error) : func(m_value!);

	/// <summary>
	/// Check service results for equivalence.
	/// </summary>
	public bool IsEquivalentTo(ServiceResult<T>? other) => base.IsEquivalentTo(other);

	/// <summary>
	/// Render result as a string.
	/// </summary>
	public override string ToString() => IsSuccess ? $"<Success={m_value}>" : base.ToString();

	internal ServiceResult(T value)
		: base(null)
	{
		m_value = value;
	}

	internal override Type InternalValueType => typeof(T);

	internal override object? InternalValue => m_value;

	internal override bool IsInternalValueEquivalent(ServiceResult other)
	{
		var otherOfT = other as ServiceResult<T>;
		if (otherOfT == null)
			return false;

		return ServiceDataUtility.AreEquivalentFieldValues(m_value, otherOfT.m_value);
	}

	internal override bool ValidateInternalValue(out string? errorMessage) =>
		ServiceDataUtility.ValidateFieldValue(m_value, out errorMessage);

	private ServiceResult(ServiceErrorDto? error)
		: base(error)
	{
	}

	private readonly T? m_value;
}

/// <summary>
/// A failed service result.
/// </summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
[System.Text.Json.Serialization.JsonConverter(typeof(ServiceResultSystemTextJsonConverter))]
[MessagePack.MessagePackFormatter(typeof(ServiceResultFailureMessagePackFormatter))]
public sealed class ServiceResultFailure : ServiceResult
{
	internal ServiceResultFailure(ServiceErrorDto error)
		: base(error)
	{
	}

	/// <summary>
	/// Check service results for equivalence.
	/// </summary>
	public bool IsEquivalentTo(ServiceResultFailure? other) => base.IsEquivalentTo(other);
}
