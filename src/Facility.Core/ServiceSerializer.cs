using System.Diagnostics.CodeAnalysis;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values to and from a serialization format.
/// </summary>
public abstract class ServiceSerializer
{
	public abstract string DefaultMediaType { get; }

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public abstract string ToString(object? value);

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public virtual byte[] ToBytes(object? value)
	{
		var memoryStream = new MemoryStream();
		ToStream(value, memoryStream);
		return memoryStream.ToArray();
	}

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public abstract void ToStream(object? value, Stream outputStream);

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public virtual Task ToStreamAsync(object? value, Stream outputStream, CancellationToken cancellationToken)
	{
		ToStream(value, outputStream);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public abstract object? FromString(string stringValue, Type type);

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public virtual T? FromString<T>(string stringValue) => (T?) FromString(stringValue, typeof(T));

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public virtual object? FromBytes(byte[] bytesValue, Type type) => FromStream(new MemoryStream(bytesValue), type);

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public virtual T? FromBytes<T>(byte[] bytesValue) => (T?) FromBytes(bytesValue, typeof(T));

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public abstract object? FromStream(Stream stream, Type type);

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public virtual Task<object?> FromStreamAsync(Stream stream, Type type, CancellationToken cancellationToken) => Task.FromResult(FromStream(stream, type));

	/// <summary>
	/// Serializes a value to a <see cref="ServiceObject"/> representation of the serialization format.
	/// </summary>
	[return: NotNullIfNotNull("value")]
	public abstract ServiceObject? ToServiceObject(object? value);

	/// <summary>
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of the serialization format.
	/// </summary>
	[return: NotNullIfNotNull("serviceObject")]
	public abstract object? FromServiceObject(ServiceObject? serviceObject, Type type);

	/// <summary>
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of the serialization format.
	/// </summary>
	[return: NotNullIfNotNull("serviceObject")]
	public virtual T? FromServiceObject<T>(ServiceObject? serviceObject) => (T?) FromServiceObject(serviceObject, typeof(T));

	public virtual bool IsSupportedMediaType(string mediaType) => mediaType == DefaultMediaType;

	public override string ToString() => GetType().Name;

	/// <summary>
	/// The legacy serializer, <see cref="NewtonsoftJsonServiceSerializer"/>.
	/// </summary>
	/// <remarks>This must be used by default to support older client libraries that don't support
	/// <c>System.Text.Json</c>.</remarks>
	internal static ServiceSerializer Legacy => NewtonsoftJsonServiceSerializer.Instance;
}
