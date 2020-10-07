using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Facility.Core.Assertions
{
	/// <summary>
	/// Contains assertions for <see cref="ServiceResult" />.
	/// </summary>
	public class ServiceResultAssertions : ServiceResultAssertionsBase<ServiceResult, ServiceResultAssertions>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceResultAssertions(ServiceResult? subject)
			: base(subject)
		{
		}
	}

	/// <summary>
	/// Contains assertions for <see cref="ServiceResult{T}" />.
	/// </summary>
	[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
	public class ServiceResultAssertions<T> : ServiceResultAssertionsBase<ServiceResult<T>, ServiceResultAssertions<T>>
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		public ServiceResultAssertions(ServiceResult<T>? subject)
			: base(subject)
		{
		}

		/// <summary>
		/// Asserts that the subject result is a success whose value is equivalent to the expected value.
		/// </summary>
		public AndConstraint<ServiceResultAssertions<T>> BeSuccess(T expectedValue)
		{
			BeSuccess();
			Execute.Assertion
				.ForCondition(Subject!.IsEquivalentTo(ServiceResult.Success(expectedValue)))
				.FailWith("Expected {context:service result} to be success with value\n {0}\n  but found value\n {1}", expectedValue, Subject!.Value);
			return new AndConstraint<ServiceResultAssertions<T>>(this);
		}
	}
}
