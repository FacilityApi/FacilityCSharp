using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Facility.Core.Assertions;

/// <summary>
/// Contains assertions for <see cref="ServiceResult" />.
/// </summary>
public abstract class ServiceResultAssertionsBase<TServiceResult, TAssertions> : ReferenceTypeAssertions<TServiceResult?, TAssertions>
	where TAssertions : ServiceResultAssertionsBase<TServiceResult, TAssertions>
	where TServiceResult : ServiceResult
{
	/// <summary>
	/// Creates an instance with the specified subject.
	/// </summary>
	protected ServiceResultAssertionsBase(TServiceResult? subject)
		: base(subject)
	{
	}

	/// <summary>
	/// Asserts that the subject result is equivalent to the expected result.
	/// </summary>
	public AndConstraint<TAssertions> BeResult(TServiceResult? expected)
	{
		Execute.Assertion
			.ForCondition(ServiceDataUtility.AreEquivalentResults(Subject, expected))
			.FailWith("Expected {context:service result} to be \n {0}\n  but found\n {1}", expected, Subject);
		return new AndConstraint<TAssertions>((TAssertions) this);
	}

	/// <summary>
	/// Asserts that the subject result is a success.
	/// </summary>
	public AndConstraint<TAssertions> BeSuccess()
	{
		Execute.Assertion
			.ForCondition(Subject != null)
			.FailWith("Expected {context:service result} to be success but found null\n {0}", Subject);
		Execute.Assertion
			.ForCondition(Subject!.IsSuccess)
			.FailWith("Expected {context:service result} to be success but found failure\n {0}", Subject);
		return new AndConstraint<TAssertions>((TAssertions) this);
	}

	/// <summary>
	/// Asserts that the subject result is a failure.
	/// </summary>
	public AndConstraint<TAssertions> BeFailure()
	{
		Execute.Assertion
			.ForCondition(Subject != null)
			.FailWith("Expected {context:service result} to be failure but found null\n {0}", Subject);
		Execute.Assertion
			.ForCondition(Subject!.IsFailure)
			.FailWith("Expected {context:service result} to be failure but found success\n {0}", Subject);
		return new AndConstraint<TAssertions>((TAssertions) this);
	}

	/// <summary>
	/// Asserts that the subject result is a failure whose error is equivalent to the expected error.
	/// </summary>
	public AndConstraint<TAssertions> BeFailure(ServiceErrorDto expectedError)
	{
		BeFailure();
		Execute.Assertion
			.ForCondition(Subject!.Error?.IsEquivalentTo(expectedError) == true)
			.FailWith("Expected {context:service result} to be failure with error\n {0}\n  but found error\n {1}", expectedError, Subject!.Error);
		return new AndConstraint<TAssertions>((TAssertions) this);
	}

	/// <summary>
	/// Asserts that the subject result is a failure whose error code matches the expected error code.
	/// </summary>
	public AndConstraint<TAssertions> BeFailure(string expectedErrorCode)
	{
		BeFailure();
		Execute.Assertion
			.ForCondition(Subject!.Error?.Code == expectedErrorCode)
			.FailWith("Expected {context:service result} to be failure with error code\n {0}\n  but found error\n {1}", expectedErrorCode, Subject!.Error);
		return new AndConstraint<TAssertions>((TAssertions) this);
	}

	/// <summary>
	/// The type of the subject.
	/// </summary>
	protected override string Identifier => "service result";
}
