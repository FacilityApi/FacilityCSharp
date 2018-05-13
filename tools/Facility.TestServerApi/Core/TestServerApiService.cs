using System;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;

namespace Facility.TestServerApi.Core
{
	public sealed class TestServerApiService : ITestServerApi
	{
		public async Task<ServiceResult<GetApiInfoResponseDto>> GetApiInfoAsync(GetApiInfoRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return ServiceResult.Success(
				new GetApiInfoResponseDto
				{
					Service = "TestServerApi",
					Version = "0.1.0",
				});
		}

		public async Task<ServiceResult<CreateWidgetResponseDto>> CreateWidgetAsync(CreateWidgetRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (request.Widget == null)
				throw new ArgumentException("Widget cannot be null.", nameof(request));

			if (request.Widget.Id != null)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest("Widget 'id' must not be specified."));
			if (string.IsNullOrEmpty(request.Widget.Name))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest("Widget 'name' is missing or empty."));

			return ServiceResult.Success(
				new CreateWidgetResponseDto
				{
					Widget = new WidgetDto
					{
						Id = 1337,
						Name = request.Widget.Name
					},
					Url = "http://example.com/widgets/1337",
					ETag = "\"initial\"",
				});
		}
	}
}
