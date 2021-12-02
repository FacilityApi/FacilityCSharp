using FluentAssertions.Collections;

namespace Facility.Core.Assertions;

/// <summary>
/// Extension methods for use with FluentAssertions and Facility.
/// </summary>
public static class AssertionsUtility
{
	/// <summary>
	/// Returns a <see cref="ServiceDtoAssertions" /> object that can be used to assert the current <see cref="ServiceDto" />.
	/// </summary>
	public static ServiceDtoAssertions Should(this ServiceDto? dto)
	{
		return new ServiceDtoAssertions(dto);
	}

	/// <summary>
	/// Returns a <see cref="ServiceDtoAssertions{T}" /> object that can be used to assert the current <see cref="ServiceDto{T}" />.
	/// </summary>
	public static ServiceDtoAssertions<T> Should<T>(this ServiceDto<T>? dto)
		where T : ServiceDto<T>
	{
		return new ServiceDtoAssertions<T>(dto);
	}

	/// <summary>
	/// Returns a <see cref="ServiceResultAssertions" /> object that can be used to assert the current <see cref="ServiceResult" />.
	/// </summary>
	public static ServiceResultAssertions Should(this ServiceResult? result)
	{
		return new ServiceResultAssertions(result);
	}

	/// <summary>
	/// Returns a <see cref="ServiceResultAssertions{T}" /> object that can be used to assert the current <see cref="ServiceResult{T}" />.
	/// </summary>
	public static ServiceResultAssertions<T> Should<T>(this ServiceResult<T>? result)
	{
		return new ServiceResultAssertions<T>(result);
	}

	/// <summary>
	/// Asserts that the subject DTOs are equivalent to the expected DTOs.
	/// </summary>
	public static void BeDtos<T>(this GenericCollectionAssertions<T> assertions, params T[] expected)
		where T : ServiceDto
	{
		assertions.Equal(expected, ServiceDataUtility.AreEquivalentDtos);
	}

	/// <summary>
	/// Asserts that the subject DTOs are equivalent to the expected DTOs.
	/// </summary>
	public static void BeDtos<T>(this GenericCollectionAssertions<T> assertions, IEnumerable<T> expected)
		where T : ServiceDto
	{
		assertions.Equal(expected, ServiceDataUtility.AreEquivalentDtos);
	}

	/// <summary>
	/// Asserts that the subject results are equivalent to the expected results.
	/// </summary>
	public static void BeResults<T>(this GenericCollectionAssertions<T> assertions, params T[] expected)
		where T : ServiceResult
	{
		assertions.Equal(expected, ServiceDataUtility.AreEquivalentResults);
	}

	/// <summary>
	/// Asserts that the subject results are equivalent to the expected results.
	/// </summary>
	public static void BeResults<T>(this GenericCollectionAssertions<T> assertions, IEnumerable<T> expected)
		where T : ServiceResult
	{
		assertions.Equal(expected, ServiceDataUtility.AreEquivalentResults);
	}
}
