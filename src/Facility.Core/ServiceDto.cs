namespace Facility.Core
{
	/// <summary>
	/// Base class for data objects used by services.
	/// </summary>
	public abstract class ServiceDto : IServiceData<ServiceDto>
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

		/// <summary>
		/// True if the data is equivalent.
		/// </summary>
		bool IServiceData.IsEquivalentTo(IServiceData other)
		{
			return IsEquivalentTo(other as ServiceDto);
		}
	}

	/// <summary>
	/// Base class for data objects used by services.
	/// </summary>
	public abstract class ServiceDto<T> : ServiceDto, IServiceData<T>
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
