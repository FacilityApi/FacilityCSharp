using System.Collections.Generic;
using Facility.Core;
using FluentAssertions;
using FluentAssertions.Collections;

namespace Facility.Core.Assertions
{
	/// <summary>
	/// Contains assertions for <see cref="ServiceResult" /> collections.
	/// </summary>
	public abstract class ServiceResultCollectionAssertionsBase<TServiceResult, TAssertions> : SelfReferencingCollectionAssertions<TServiceResult, TAssertions>
		where TAssertions : ServiceResultCollectionAssertionsBase<TServiceResult, TAssertions>
		where TServiceResult : ServiceResult
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		protected ServiceResultCollectionAssertionsBase(IEnumerable<TServiceResult> expected)
			: base(expected)
		{
		}

		/// <summary>
		/// Asserts that the subject results are equivalent to the expected results.
		/// </summary>
		public AndConstraint<TAssertions> BeResults(params TServiceResult[] expected)
		{
			Equal(expected, ServiceDataUtility.AreEquivalentResults);
			return new AndConstraint<TAssertions>((TAssertions) this);
		}

		/// <summary>
		/// Asserts that the subject results are equivalent to the expected results.
		/// </summary>
		public AndConstraint<TAssertions> BeResults(IEnumerable<TServiceResult> expected)
		{
			Equal(expected, ServiceDataUtility.AreEquivalentResults);
			return new AndConstraint<TAssertions>((TAssertions) this);
		}
	}

	/// <summary>
	/// Contains assertions for <see cref="ServiceResult" /> collections.
	/// </summary>
	public class ServiceResultCollectionAssertions : ServiceResultCollectionAssertionsBase<ServiceResult, ServiceResultCollectionAssertions>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceResultCollectionAssertions(IEnumerable<ServiceResult> expected)
			: base(expected)
		{
		}
	}

	/// <summary>
	/// Contains assertions for <see cref="ServiceResult{T}" /> collections.
	/// </summary>
	public class ServiceResultCollectionAssertions<T> : ServiceResultCollectionAssertionsBase<ServiceResult<T>, ServiceResultCollectionAssertions<T>>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceResultCollectionAssertions(IEnumerable<ServiceResult<T>> expected)
			: base(expected)
		{
		}
	}
}
