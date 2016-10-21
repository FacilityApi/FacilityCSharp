namespace Facility.Core
{
	/// <summary>
	/// Common interface for service data elements.
	/// </summary>
	public interface IServiceData
	{
		/// <summary>
		/// Checks for equivalence.
		/// </summary>
		bool IsEquivalentTo(IServiceData other);
	}

	/// <summary>
	/// Common interface for service data elements.
	/// </summary>
	public interface IServiceData<T> : IServiceData
	{
		/// <summary>
		/// Checks for equivalence.
		/// </summary>
		bool IsEquivalentTo(T other);
	}
}
