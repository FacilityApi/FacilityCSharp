namespace Facility.Core
{
	/// <summary>
	/// An error.
	/// </summary>
	public sealed partial class ServiceErrorDto : ServiceDto<ServiceErrorDto>
	{
		/// <summary>
		/// Creates a service error.
		/// </summary>
		public ServiceErrorDto()
		{
		}

		/// <summary>
		/// Creates a service error.
		/// </summary>
		public ServiceErrorDto(string code)
		{
			Code = code;
		}

		/// <summary>
		/// Creates a service error.
		/// </summary>
		public ServiceErrorDto(string code, string message)
		{
			Code = code;
			Message = message;
		}
	}
}
