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

			if (request.Widget.Name != c_shinyWidgetName)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest("Widget 'name' must be 'shiny'."));

			return ServiceResult.Success(
				new CreateWidgetResponseDto
				{
					Widget = new WidgetDto
					{
						Id = c_shinyWidgetId,
						Name = c_shinyWidgetName,
					},
					Url = "/widgets/1337",
					ETag = c_initialETag,
				});
		}

		public async Task<ServiceResult<GetWidgetResponseDto>> GetWidgetAsync(GetWidgetRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (request.Id == null)
				throw new ArgumentException("ID cannot be null.", nameof(request));

			if (request.Id != c_shinyWidgetId)
				return ServiceResult.Failure(ServiceErrors.CreateNotFound());

			if (request.IfNotETag == c_initialETag)
				return ServiceResult.Success(new GetWidgetResponseDto { NotModified = true });

			return ServiceResult.Success(
				new GetWidgetResponseDto
				{
					Widget = new WidgetDto
					{
						Id = c_shinyWidgetId,
						Name = c_shinyWidgetName,
					},
					ETag = c_initialETag,
				});
		}

		private const string c_initialETag = "\"initial\"";
		private const string c_shinyWidgetName = "shiny";
		private const int c_shinyWidgetId = 1337;
	}
}
