using System.Collections.Generic;
using Facility.Core;

namespace Facility.Core.Assertions
{
	/// <summary>
	/// Extention methods for use with FluentAssertions and Facility.
	/// </summary>
	public static class AssertionsUtility
	{
		/// <summary>
		/// Returns a <see cref="ServiceDtoAssertions" /> object that can be used to assert the current <see cref="ServiceDto" />.
		/// </summary>
		public static ServiceDtoAssertions Should(this ServiceDto dto)
		{
			return new ServiceDtoAssertions(dto);
		}

		/// <summary>
		/// Returns a <see cref="ServiceDtoAssertions{T}" /> object that can be used to assert the current <see cref="ServiceDto{T}" />.
		/// </summary>
		public static ServiceDtoAssertions<T> Should<T>(this ServiceDto<T> dto)
			where T : ServiceDto<T>
		{
			return new ServiceDtoAssertions<T>(dto);
		}

		/// <summary>
		/// Returns a <see cref="ServiceDtoCollectionAssertions" /> object that can be used to assert the current <see cref="ServiceDto" /> collection.
		/// </summary>
		public static ServiceDtoCollectionAssertions Should(this IEnumerable<ServiceDto> dtos)
		{
			return new ServiceDtoCollectionAssertions(dtos);
		}

		/// <summary>
		/// Returns a <see cref="ServiceDtoCollectionAssertions{T}" /> object that can be used to assert the current <see cref="ServiceDto{T}" /> collection.
		/// </summary>
		public static ServiceDtoCollectionAssertions<T> Should<T>(this IEnumerable<ServiceDto<T>> dtos)
			where T : ServiceDto<T>
		{
			return new ServiceDtoCollectionAssertions<T>(dtos);
		}

		/// <summary>
		/// Returns a <see cref="ServiceResultAssertions" /> object that can be used to assert the current <see cref="ServiceResult" />.
		/// </summary>
		public static ServiceResultAssertions Should(this ServiceResult result)
		{
			return new ServiceResultAssertions(result);
		}

		/// <summary>
		/// Returns a <see cref="ServiceResultAssertions{T}" /> object that can be used to assert the current <see cref="ServiceResult{T}" />.
		/// </summary>
		public static ServiceResultAssertions<T> Should<T>(this ServiceResult<T> result)
		{
			return new ServiceResultAssertions<T>(result);
		}

		/// <summary>
		/// Returns a <see cref="ServiceResultCollectionAssertions" /> object that can be used to assert the current <see cref="ServiceResult" /> collection.
		/// </summary>
		public static ServiceResultCollectionAssertions Should(this IEnumerable<ServiceResult> results)
		{
			return new ServiceResultCollectionAssertions(results);
		}

		/// <summary>
		/// Returns a <see cref="ServiceResultCollectionAssertions" /> object that can be used to assert the current <see cref="ServiceResult" /> collection.
		/// </summary>
		public static ServiceResultCollectionAssertions<T> Should<T>(this IEnumerable<ServiceResult<T>> results)
		{
			return new ServiceResultCollectionAssertions<T>(results);
		}
	}
}
