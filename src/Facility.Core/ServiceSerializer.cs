using System.Diagnostics.CodeAnalysis;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values.
/// </summary>
public abstract class ServiceSerializer
{
	/// <summary>
	/// The media type used by default.
	/// </summary>
	public abstract string DefaultMediaType { get; }

	/// <summary>
	/// Serializes a value.
	/// </summary>
	public abstract Task ToStreamAsync(object? value, Stream stream, CancellationToken cancellationToken);

	/// <summary>
	/// Deserializes a value.
	/// </summary>
	public abstract Task<object?> FromStreamAsync(Stream stream, Type type, CancellationToken cancellationToken);

	/// <summary>
	/// Clones a value by serializing and deserializing.
	/// </summary>
	[return: NotNullIfNotNull("value")]
	public abstract T Clone<T>(T value);
}
