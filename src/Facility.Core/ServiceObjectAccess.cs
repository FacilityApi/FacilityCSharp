namespace Facility.Core;

/// <summary>
/// Specifies how the <see cref="ServiceObject"/> data is accessed.
/// </summary>
public enum ServiceObjectAccess
{
	/// <summary>
	/// Clones the data. If the returned object is mutated, the <c>ServiceObject</c> is not affected, and vice versa.
	/// </summary>
	Clone,

	/// <summary>
	/// Returns mutable object data. If the returned object is mutated, the <c>ServiceObject</c> is mutated.
	/// </summary>
	/// <remarks>If the returned object is mutated after another access method is called on the same
	/// <c>ServiceObject</c>, the <c>ServiceObject</c> may or may not be affected, and vice versa.</remarks>
	ReadWrite,

	/// <summary>
	/// Returns object data that must not be mutated, nor accessed after the <c>ServiceObject</c> is mutated.
	/// </summary>
	/// <remarks>If the returned object is mutated, the <c>ServiceObject</c> may or may not be affected,
	/// and vice versa.</remarks>
	ReadOnly,
}
