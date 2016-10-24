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
		public override string ToString()
		{
			return ServiceJsonUtility.ToJson(this);
		}

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public abstract bool IsEquivalentTo(ServiceDto other);
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
		public sealed override bool IsEquivalentTo(ServiceDto other)
		{
			return IsEquivalentTo(other as T);
		}

		/// <summary>
		/// Determines if two DTOs are equivalent.
		/// </summary>
		public abstract bool IsEquivalentTo(T other);
	}
}
