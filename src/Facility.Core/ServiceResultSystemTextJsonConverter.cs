using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Facility.Core;

public sealed class ServiceResultSystemTextJsonConverter : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeof(ServiceResult).IsAssignableFrom(typeToConvert);

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		if (typeToConvert == typeof(ServiceResultFailure))
			return new ServiceResultSystemTextJsonConverter<ServiceResultFailure>();
		if (typeToConvert == typeof(ServiceResult))
			return new ServiceResultSystemTextJsonConverter<ServiceResult>();
		var valueType = typeToConvert.GetGenericArguments()[0];
		return (JsonConverter) Activator.CreateInstance(typeof(ServiceResultSystemTextJsonConverter<>).MakeGenericType(typeof(ServiceResult<>).MakeGenericType(valueType)))!;
	}
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
public sealed class ServiceResultSystemTextJsonConverter<TServiceResult> : JsonConverter<TServiceResult>
	where TServiceResult : ServiceResult
{
	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	public override TServiceResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		MatchTokenOrThrow(ref reader, JsonTokenType.StartObject);
		ReadOrThrow(ref reader);

		var valueType = typeToConvert.IsConstructedGenericType ? typeToConvert.GenericTypeArguments[0] : null;
		object? value = null;
		ServiceErrorDto? error = null;

		while (reader.TokenType == JsonTokenType.PropertyName)
		{
			string propertyName = reader.GetString()!;
			ReadOrThrow(ref reader);

			if (string.Equals(propertyName, c_valuePropertyName, StringComparison.OrdinalIgnoreCase))
			{
				if (valueType == null)
					throw new JsonException("ServiceResult does not support 'value'; use ServiceResult<T>.");

				value = JsonSerializer.Deserialize(ref reader, valueType, options);
			}
			else if (string.Equals(propertyName, c_errorPropertyName, StringComparison.OrdinalIgnoreCase))
			{
				error = JsonSerializer.Deserialize<ServiceErrorDto>(ref reader, options);
			}

			ReadOrThrow(ref reader);
		}

		MatchTokenOrThrow(ref reader, JsonTokenType.EndObject);

		if (value != null && error != null)
			throw new JsonException("ServiceResult must not have both 'value' and 'error'.");

		if (valueType == null)
		{
			return (TServiceResult) (error != null ? ServiceResult.Failure(error) : ServiceResult.Success());
		}
		if (error != null)
		{
			return (TServiceResult) s_genericCastMethod.MakeGenericMethod(valueType).Invoke(ServiceResult.Failure(error), Array.Empty<object>())!;
		}
		else
		{
			if (value == null && valueType.GetTypeInfo().IsValueType)
				value = Activator.CreateInstance(valueType);
			return (TServiceResult) s_genericSuccessMethod.MakeGenericMethod(valueType).Invoke(null, new[] { value })!;
		}
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	public override void Write(Utf8JsonWriter writer, TServiceResult value, JsonSerializerOptions options)
	{
		var valueType = value.InternalValueType;

		writer.WriteStartObject();
		if (value.IsFailure)
		{
			writer.WritePropertyName(c_errorPropertyName);
			JsonSerializer.Serialize(writer, value.Error, options);
		}
		else if (valueType != null)
		{
			writer.WritePropertyName(c_valuePropertyName);
			JsonSerializer.Serialize(writer, value.InternalValue, options);
		}
		writer.WriteEndObject();
	}

	private static void ReadOrThrow(ref Utf8JsonReader reader)
	{
		if (!reader.Read())
			throw new JsonException("ServiceResult JSON ended unexpectedly.");
	}

	private static void MatchTokenOrThrow(ref Utf8JsonReader reader, JsonTokenType tokenType)
	{
		if (reader.TokenType != tokenType)
			throw new JsonException($"ServiceResult expected {tokenType} but found {reader.TokenType}.");
	}

	private const string c_valuePropertyName = "value";
	private const string c_errorPropertyName = "error";
	private static readonly MethodInfo s_genericSuccessMethod = typeof(ServiceResult).GetRuntimeMethods().First(x => x.Name == "Success" && x.IsStatic && x.IsGenericMethodDefinition);
	private static readonly MethodInfo s_genericCastMethod = typeof(ServiceResult).GetRuntimeMethods().First(x => x.Name == "Cast" && !x.IsStatic && x.IsGenericMethodDefinition);
}
