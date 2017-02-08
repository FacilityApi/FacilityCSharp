using System.Collections.Generic;
using Facility.Core;
using FluentAssertions;
using FluentAssertions.Collections;

namespace Facility.Core.Assertions
{
	/// <summary>
	/// Contains assertions for <see cref="ServiceDto" /> collections.
	/// </summary>
	public abstract class ServiceDtoCollectionAssertionsBase<TServiceDto, TAssertions> : SelfReferencingCollectionAssertions<TServiceDto, TAssertions>
		where TAssertions : ServiceDtoCollectionAssertionsBase<TServiceDto, TAssertions>
		where TServiceDto : ServiceDto
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		protected ServiceDtoCollectionAssertionsBase(IEnumerable<TServiceDto> expected)
			: base(expected)
		{
		}

		/// <summary>
		/// Asserts that the subject results are equivalent to the expected results.
		/// </summary>
		public AndConstraint<TAssertions> BeDtos(params TServiceDto[] expected)
		{
			Equal(expected, ServiceDataUtility.AreEquivalentDtos);
			return new AndConstraint<TAssertions>((TAssertions) this);
		}

		/// <summary>
		/// Asserts that the subject results are equivalent to the expected results.
		/// </summary>
		public AndConstraint<TAssertions> BeDtos(IEnumerable<TServiceDto> expected)
		{
			Equal(expected, ServiceDataUtility.AreEquivalentDtos);
			return new AndConstraint<TAssertions>((TAssertions) this);
		}
	}

	/// <summary>
	/// Contains assertions for <see cref="ServiceDto" /> collections.
	/// </summary>
	public class ServiceDtoCollectionAssertions : ServiceDtoCollectionAssertionsBase<ServiceDto, ServiceDtoCollectionAssertions>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceDtoCollectionAssertions(IEnumerable<ServiceDto> expected)
			: base(expected)
		{
		}
	}

	/// <summary>
	/// Contains assertions for <see cref="ServiceDto{T}" /> collections.
	/// </summary>
	public class ServiceDtoCollectionAssertions<T> : ServiceDtoCollectionAssertionsBase<ServiceDto<T>, ServiceDtoCollectionAssertions<T>>
		where T : ServiceDto<T>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceDtoCollectionAssertions(IEnumerable<ServiceDto<T>> expected)
			: base(expected)
		{
		}
	}
}
