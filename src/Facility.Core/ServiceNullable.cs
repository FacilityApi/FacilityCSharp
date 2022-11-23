using System.Diagnostics.CodeAnalysis;

namespace Facility.Core;

/// <summary>
/// Used to distinguish unspecified from null.
/// </summary>
[Newtonsoft.Json.JsonConverter(typeof(ServiceNullableNewtonsoftJsonConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(ServiceNullableSystemTextJsonConverter))]
public readonly struct ServiceNullable<T> : IEquatable<ServiceNullable<T>>, IServiceNullable
{
	/// <summary>
	/// Creates a non-default instance.
	/// </summary>
	public ServiceNullable(T? value)
	{
		m_value = value;
		m_isSpecified = true;
	}

	/// <summary>
	/// Implicitly creates a non-default instance from a (possibly null) value.
	/// </summary>
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Use constructor.")]
	public static implicit operator ServiceNullable<T>(T? value) => new(value);

	/// <summary>
	/// True if this instance is unspecified.
	/// </summary>
	public bool IsUnspecified => !m_isSpecified;

	/// <summary>
	/// True if this instance has a null value.
	/// </summary>
	public bool IsNull => m_isSpecified && m_value is null;

	/// <summary>
	/// The value of this instance, or null/default if the instance is unspecified.
	/// </summary>
	/// <remarks>This property does not throw an exception when unspecified or null.
	/// Check <see cref="IsUnspecified" /> to distinguish unspecified from null.</remarks>
	public T? Value => m_value;

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	public bool Equals(ServiceNullable<T> other) =>
		(m_isSpecified && other.m_isSpecified) ? ServiceDataUtility.AreEquivalentFieldValues(m_value, other.m_value) : (m_isSpecified == other.m_isSpecified);

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	public override bool Equals(object? obj) => obj is ServiceNullable<T> other && Equals(other);

	/// <summary>
	/// Retrieves the hash code of the object.
	/// </summary>
	public override int GetHashCode() => m_isSpecified ? (m_value?.GetHashCode() ?? 0) : -1;

	/// <summary>
	/// Returns the text representation of the object.
	/// </summary>
	public override string ToString() => m_value?.ToString() ?? "";

	/// <summary>
	/// Compares two instances for equality.
	/// </summary>
	public static bool operator ==(ServiceNullable<T> left, ServiceNullable<T> right) => left.Equals(right);

	/// <summary>
	/// Compares two instances for inequality.
	/// </summary>
	public static bool operator !=(ServiceNullable<T> left, ServiceNullable<T> right) => !left.Equals(right);

	object? IServiceNullable.Value => Value;

	private readonly T? m_value;
	private readonly bool m_isSpecified;
}
