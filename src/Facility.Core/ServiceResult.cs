using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Facility.Core
{
	/// <summary>
	/// A service result success or error.
	/// </summary>
	[JsonConverter(typeof(ServiceResultJsonConverter))]
	public class ServiceResult
	{
		/// <summary>
		/// Creates a successful result.
		/// </summary>
		public static ServiceResult Success()
		{
			return new ServiceResult(null);
		}

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		public static ServiceResult<T> Success<T>(T value)
		{
			return new ServiceResult<T>(value);
		}

		/// <summary>
		/// Creates a failed result.
		/// </summary>
		public static ServiceResultFailure Failure(ServiceErrorDto error)
		{
			if (error == null)
				throw new ArgumentNullException(nameof(error));

			return new ServiceResultFailure(error);
		}

		/// <summary>
		/// True if the result has a value.
		/// </summary>
		public bool IsSuccess => m_error == null;

		/// <summary>
		/// True if the result has an error.
		/// </summary>
		public bool IsFailure => m_error != null;

		/// <summary>
		/// The error.
		/// </summary>
		public ServiceErrorDto Error => m_error;

		/// <summary>
		/// Throws a ServiceException if the result is an error.
		/// </summary>
		public void Verify()
		{
			if (m_error != null)
				throw new ServiceException(m_error);
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
				return Failure(m_error);
			else
				return Success((T) InternalValue);
		}

		/// <summary>
		/// The service result as a failure; null if it is a success.
		/// </summary>
		public ServiceResultFailure AsFailure()
		{
			return this as ServiceResultFailure ?? (IsFailure ? Failure(m_error) : null);
		}

		/// <summary>
		/// Check service results for equivalence.
		/// </summary>
		public bool IsEquivalentTo(ServiceResult other)
		{
			if (other == null)
				return false;

			if (IsFailure)
				return other.IsFailure && ServiceDataUtility.AreEquivalentDtos(m_error, other.m_error);

			Type valueType = InternalValueType;
			if (valueType == null)
				return other.InternalValueType == null;

			return IsInternalValueEquivalent(other);
		}

		/// <summary>
		/// Render result as a string.
		/// </summary>
		public override string ToString()
		{
			return IsSuccess ? "<Success>" : $"<Failure={m_error}>";
		}

		/// <summary>
		/// Used for JSON serialization.
		/// </summary>
		public sealed class ServiceResultJsonConverter : JsonConverter
		{
			/// <summary>
			/// Determines whether this instance can convert the specified object type.
			/// </summary>
			public override bool CanConvert(Type objectType)
			{
				return objectType.GetTypeInfo().IsAssignableFrom(typeof(ServiceResult).GetTypeInfo());
			}

			/// <summary>
			/// Reads the JSON representation of the object.
			/// </summary>
			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (reader.TokenType == JsonToken.Null)
					return null;

				MatchTokenOrThrow(reader, JsonToken.StartObject);
				ReadOrThrow(reader);

				Type valueType = objectType.IsConstructedGenericType ? objectType.GenericTypeArguments[0] : null;
				object value = null;
				ServiceErrorDto error = null;

				while (reader.TokenType == JsonToken.PropertyName)
				{
					string propertyName = (string) reader.Value;
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
					return (ServiceResult) s_genericCastMethod.MakeGenericMethod(valueType).Invoke(Failure(error), new object[0]);
				}
				else
				{
					if (value == null && valueType.GetTypeInfo().IsValueType)
						value = Activator.CreateInstance(valueType);
					return (ServiceResult) s_genericSuccessMethod.MakeGenericMethod(valueType).Invoke(null, new[] { value });
				}
			}

			/// <summary>
			/// Writes the JSON representation of the object.
			/// </summary>
			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				ServiceResult serviceResult = (ServiceResult) value;
				Type valueType = serviceResult.InternalValueType;

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

			const string c_valuePropertyName = "value";
			const string c_errorPropertyName = "error";

			static readonly MethodInfo s_genericSuccessMethod = typeof(ServiceResult).GetRuntimeMethods().First(x => x.Name == "Success" && x.IsStatic && x.IsGenericMethodDefinition);
			static readonly MethodInfo s_genericCastMethod = typeof(ServiceResult).GetRuntimeMethods().First(x => x.Name == "Cast" && !x.IsStatic && x.IsGenericMethodDefinition);
		}

		internal ServiceResult(ServiceErrorDto error)
		{
			m_error = error;
		}

		internal virtual Type InternalValueType => null;

		internal virtual object InternalValue
		{
			get { throw new InvalidCastException("A successful result without a value cannot be cast."); }
		}

		internal virtual bool IsInternalValueEquivalent(ServiceResult result)
		{
			return false;
		}

		readonly ServiceErrorDto m_error;
	}

	/// <summary>
	/// A service result value or error.
	/// </summary>
	public sealed class ServiceResult<T> : ServiceResult
	{
		/// <summary>
		/// Implicitly create a failed result from an error.
		/// </summary>
		public static implicit operator ServiceResult<T>(ServiceResultFailure failure)
		{
			return new ServiceResult<T>(failure.Error);
		}

		/// <summary>
		/// The value. (Throws a ServiceException on failure.)
		/// </summary>
		public T Value
		{
			get
			{
				Verify();
				return m_value;
			}
		}

		/// <summary>
		/// The value. (Returns null on failure.)
		/// </summary>
		public T GetValueOrDefault()
		{
			return m_value;
		}

		/// <summary>
		/// Maps a ServiceResult from one type to another.
		/// </summary>
		/// <remarks>If the result is a success, the function is called on the input value to produce
		/// a successful service result matching the type of the output value. If the result is a failure,
		/// the function is not called, and a failed service result using the output type is returned.</remarks>
		public ServiceResult<TOutput> Map<TOutput>(Func<T, TOutput> func)
		{
			return Map(x => new ServiceResult<TOutput>(func(x)));
		}

		/// <summary>
		/// Maps a ServiceResult from one type to another.
		/// </summary>
		/// <remarks>If the result is a success, the function is called on the input value to produce
		/// a service result matching the type of the output value. If the result is a failure,
		/// the function is not called, and a failed service result using the output type is returned.</remarks>
		public ServiceResult<TOutput> Map<TOutput>(Func<T, ServiceResult<TOutput>> func)
		{
			return IsFailure ? new ServiceResult<TOutput>(Error) : func(m_value);
		}

		/// <summary>
		/// Check service results for equivalence.
		/// </summary>
		public bool IsEquivalentTo(ServiceResult<T> other)
		{
			return base.IsEquivalentTo(other);
		}

		/// <summary>
		/// Render result as a string.
		/// </summary>
		public override string ToString()
		{
			return IsSuccess ? $"<Success={m_value}>" : base.ToString();
		}

		internal ServiceResult(T value)
			: base(null)
		{
			m_value = value;
		}

		internal override Type InternalValueType => typeof(T);

		internal override object InternalValue => m_value;

		internal override bool IsInternalValueEquivalent(ServiceResult other)
		{
			var otherOfT = other as ServiceResult<T>;
			if (otherOfT == null)
				return false;

			var dto = m_value as ServiceDto;
			if (dto != null)
				return dto.IsEquivalentTo(otherOfT.m_value as ServiceDto);

			return EqualityComparer<T>.Default.Equals(m_value, otherOfT.m_value);
		}

		private ServiceResult(ServiceErrorDto error)
			: base(error)
		{
		}

		readonly T m_value;
	}

	/// <summary>
	/// A failed service result.
	/// </summary>
	public sealed class ServiceResultFailure : ServiceResult
	{
		internal ServiceResultFailure(ServiceErrorDto error)
			: base(error)
		{
		}

		/// <summary>
		/// Check service results for equivalence.
		/// </summary>
		public bool IsEquivalentTo(ServiceResultFailure other)
		{
			return base.IsEquivalentTo(other);
		}
	}
}
