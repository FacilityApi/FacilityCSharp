namespace Facility.Core;

/// <summary>
/// Common service delegators.
/// </summary>
public static class ServiceDelegators
{
	/// <summary>
	/// All methods throw <see cref="NotImplementedException"/>.
	/// </summary>
	public static ServiceDelegator NotImplemented { get; } = async (_, _, _) => throw new NotImplementedException();

	/// <summary>
	/// Forwards all methods to the inner service.
	/// </summary>
	public static ServiceDelegator Forward(object inner)
	{
		if (inner is null)
			throw new ArgumentNullException(nameof(inner));

		return (method, request, cancellationToken) => method.InvokeAsync(inner, request, cancellationToken);
	}

	/// <summary>
	/// Validates requests and responses.
	/// </summary>
	public static ServiceDelegator Validate(object inner)
	{
		if (inner is null)
			throw new ArgumentNullException(nameof(inner));

		return async (method, request, cancellationToken) =>
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			if (!request.Validate(out var requestErrorMessage))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest(requestErrorMessage));

			var response = await method.InvokeAsync(inner, request, cancellationToken).ConfigureAwait(false);

			if (!response.Validate(out var responseErrorMessage))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidResponse(responseErrorMessage));

			return response;
		};
	}
}
