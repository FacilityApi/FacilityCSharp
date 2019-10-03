using System;
using System.Collections.Generic;

namespace Facility.Core
{
	/// <summary>
	/// Base class for data objects used by services.
	/// </summary>
	public abstract class ServiceDto
	{
		/// <summary>
		/// Returns the DTO as JSON.
		/// </summary>
		public override string ToString() => ServiceJsonUtility.ToJson(this);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public abstract bool IsEquivalentTo(ServiceDto other);

		/// <summary>
		/// Returns validation errors
		/// </summary>
		public virtual IEnumerable<string> GetValidationErrors() => Array.Empty<string>();
	}

	/// <summary>
	/// Base class for data objects used by services.
	/// </summary>
	public abstract class ServiceDto<T> : ServiceDto
		where T : ServiceDto<T>
	{
		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public sealed override bool IsEquivalentTo(ServiceDto other) => IsEquivalentTo(other as T);

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public abstract bool IsEquivalentTo(T other);
	}
}
