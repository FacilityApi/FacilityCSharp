using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Facility.Core
{
	/// <summary>
	/// Base class for simple JSON converters.
	/// </summary>
	/// <typeparam name="T">The type.</typeparam>
	/// <remarks>This base class provides strongly-typed abstract methods for reading and writing. The converter only
	/// supports type T directly, not derived types. Null is handled automatically for both reading and writing, including support
	/// for Nullable{T}. A JsonException is thrown if null is read for a non-nullable value type. If your converter
	/// has more advanced needs, derive from JsonConverter directly.</remarks>
	public abstract class ServiceSystemTextJsonConverterBase<T> : JsonConverter<T>
	{
		/// <summary>
		/// Implements CanConvert.
		/// </summary>
		public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T) || (s_nullableType != null && typeToConvert == s_nullableType);

		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
			{
				if (s_nullableType != null && typeToConvert != s_nullableType)
					throw new JsonException("Cannot convert null to non-nullable type " + typeToConvert.Name);
				return default;
			}

			return ReadCore(ref reader, options);
		}

		/// <summary>
		/// Reads the JSON representation of the value.
		/// </summary>
		protected abstract T ReadCore(ref Utf8JsonReader reader, JsonSerializerOptions options);

		private static readonly Type? s_nullableType = typeof(T).GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null ? typeof(Nullable<>).MakeGenericType(typeof(T)) : null;
	}
}
