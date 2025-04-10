using System.Diagnostics.CodeAnalysis;

namespace Facility.Core;

/// <summary>
/// Base class for data objects used by services.
/// </summary>
public abstract class ServiceDto
{
	/// <summary>
	/// Returns the DTO as JSON.
	/// </summary>
	public override string ToString() => JsonSerializer.ToJson(this);

	/// <summary>
	/// Determines if two DTOs are equivalent.
	/// </summary>
	public abstract bool IsEquivalentTo(ServiceDto? other);

	/// <summary>
	/// Validates the DTO.
	/// </summary>
	/// <param name="errorMessage">The error message if the DTO is invalid, <c>null</c> otherwise.</param>
	/// <returns>True if the DTO is valid.</returns>
	public virtual bool Validate(out string? errorMessage)
	{
		errorMessage = null;
		return true;
	}

	/// <summary>
	/// The JSON serializer used by <c>ToString</c> and when creating a <see cref="ServiceObject" />.
	/// </summary>
	protected virtual JsonServiceSerializer JsonSerializer => JsonServiceSerializer.Legacy;

	internal JsonServiceSerializer GetJsonSerializer() => JsonSerializer;
}

/// <summary>
/// Base class for data objects used by services.
/// </summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
public abstract class ServiceDto<T> : ServiceDto
	where T : ServiceDto<T>
{
	/// <summary>
	/// Determines if two DTOs are equivalent.
	/// </summary>
	public sealed override bool IsEquivalentTo(ServiceDto? other) => IsEquivalentTo(other as T);

	/// <summary>
	/// Determines if two DTOs are equivalent.
	/// </summary>
	public virtual bool IsEquivalentTo(T? other) => JsonSerializer.AreEquivalent(this, other);

	/// <summary>
	/// Deep clones the DTO.
	/// </summary>
	public virtual T DeepClone() => JsonSerializer.Clone((T) this);
}
