using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Facility.Core.Assertions
{
	/// <summary>
	/// Contains assertions for <see cref="ServiceDto" />.
	/// </summary>
	public abstract class ServiceDtoAssertionsBase<TServiceDto, TAssertions> : ReferenceTypeAssertions<TServiceDto?, TAssertions>
		where TAssertions : ServiceDtoAssertionsBase<TServiceDto, TAssertions>
		where TServiceDto : ServiceDto
	{
		/// <summary>
		/// Creates an instance with the specified subject.
		/// </summary>
		protected ServiceDtoAssertionsBase(TServiceDto? subject)
		{
			Subject = subject;
		}

		/// <summary>
		/// Asserts that the subject result is equivalent to the expected result.
		/// </summary>
		public AndConstraint<TAssertions> BeDto(TServiceDto? expected)
		{
			Execute.Assertion
				.ForCondition(ServiceDataUtility.AreEquivalentDtos(Subject, expected))
				.FailWith("Expected {context:DTO} to be \n {0}\n  but found\n {1}", expected, Subject);
			return new AndConstraint<TAssertions>((TAssertions) this);
		}

		/// <summary>
		/// The type of the subject.
		/// </summary>
		protected override string Identifier => "DTO";
	}
}
