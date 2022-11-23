using System.ComponentModel;

namespace Facility.Core;

/// <summary>
/// Sets the <c>DefaultValue</c> to <c>new T()</c> for the specified type.
/// </summary>
public sealed class ServiceNullableDefaultValueAttribute : DefaultValueAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ServiceNullableDefaultValueAttribute"/> class.
	/// </summary>
	/// <param name="type">The type.</param>
	public ServiceNullableDefaultValueAttribute(Type type)
		: base(Activator.CreateInstance(type))
	{
		Type = type;
	}

	/// <summary>
	/// The type.
	/// </summary>
	public Type Type { get; }
}
