namespace Facility.Core;

/// <summary>
/// Helper methods for working with <c>ServiceResult</c>.
/// </summary>
public static class ServiceResultUtility
{
	/// <summary>
	/// Attempts to get the value from a <c>ServiceResult</c>.
	/// </summary>
	public static bool TryGetValue(ServiceResult result, out object? value)
	{
		if (result is null)
			throw new ArgumentNullException(nameof(result));

		if (result.IsFailure || result.InternalValueType is null)
		{
			value = null;
			return false;
		}

		value = result.InternalValue;
		return true;
	}
}
