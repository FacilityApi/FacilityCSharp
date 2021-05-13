using System;

namespace Facility.Core
{
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
	}
}
