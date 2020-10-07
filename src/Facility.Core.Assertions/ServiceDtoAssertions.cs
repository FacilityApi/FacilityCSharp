using System.Diagnostics.CodeAnalysis;

namespace Facility.Core.Assertions
{
	/// <summary>
	/// Contains assertions for <see cref="ServiceDto" />.
	/// </summary>
	public class ServiceDtoAssertions : ServiceDtoAssertionsBase<ServiceDto, ServiceDtoAssertions>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceDtoAssertions(ServiceDto? subject)
			: base(subject)
		{
		}
	}

	/// <summary>
	/// Contains assertions for <see cref="ServiceDto{T}" />.
	/// </summary>
	[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
	public class ServiceDtoAssertions<T> : ServiceDtoAssertionsBase<ServiceDto<T>, ServiceDtoAssertions<T>>
		where T : ServiceDto<T>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceDtoAssertions(ServiceDto<T>? subject)
			: base(subject)
		{
		}
	}
}
