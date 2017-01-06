using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;

namespace Facility.ExampleApi
{
	public sealed class ExampleApiService : IExampleApi
	{
		public ExampleApiService(IExampleApiRepository repository)
		{
			m_repository = repository;
		}

		public async Task<ServiceResult<GetWidgetsResponseDto>> GetWidgetsAsync(GetWidgetsRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return ServiceResult.Success(await m_repository.GetWidgetsAsync(request, cancellationToken).ConfigureAwait(false));
		}

		public async Task<ServiceResult<CreateWidgetResponseDto>> CreateWidgetAsync(CreateWidgetRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (request.Widget == null)
				return ServiceResult.Failure(ServiceErrors.CreateRequestFieldRequired("widget"));

			var newWidget = await m_repository.CreateWidgetAsync(request.Widget, cancellationToken).ConfigureAwait(false);

			return ServiceResult.Success(new CreateWidgetResponseDto { Widget = newWidget });
		}

		public async Task<ServiceResult<GetWidgetResponseDto>> GetWidgetAsync(GetWidgetRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrEmpty(request.Id))
				return ServiceResult.Failure(ExampleApiErrors.CreateInvalidRequestMissingWidgetId());

			var widget = await m_repository.TryGetWidgetAsync(request.Id, cancellationToken).ConfigureAwait(false);

			if (widget == null)
				return ServiceResult.Failure(ExampleApiErrors.CreateNotFoundWidget(request.Id));

			string eTag = CreateWidgetETag(widget);

			if (request.IfNoneMatch == eTag)
				return ServiceResult.Success(new GetWidgetResponseDto { NotModified = true, ETag = eTag });

			return ServiceResult.Success(new GetWidgetResponseDto { Widget = widget, ETag = eTag });
		}

		public async Task<ServiceResult<DeleteWidgetResponseDto>> DeleteWidgetAsync(DeleteWidgetRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (string.IsNullOrEmpty(request.Id))
				return ServiceResult.Failure(ExampleApiErrors.CreateInvalidRequestMissingWidgetId());

			bool wasDeleted = await m_repository.TryDeleteWidgetAsync(request.Id, cancellationToken).ConfigureAwait(false);

			if (!wasDeleted)
				return ServiceResult.Failure(ExampleApiErrors.CreateNotFoundWidget(request.Id));

			return ServiceResult.Success(new DeleteWidgetResponseDto());
		}

		public async Task<ServiceResult<EditWidgetResponseDto>> EditWidgetAsync(EditWidgetRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrEmpty(request.Id))
				return ServiceResult.Failure(ExampleApiErrors.CreateInvalidRequestMissingWidgetId());

			return ServiceResult.Success(await m_repository.EditWidgetAsync(request.Id, request.Ops, cancellationToken).ConfigureAwait(false));
		}

		public async Task<ServiceResult<GetWidgetBatchResponseDto>> GetWidgetBatchAsync(GetWidgetBatchRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (request.Ids == null)
				return ServiceResult.Failure(ServiceErrors.CreateRequestFieldRequired("ids"));
			if (request.Ids.Count == 0)
				return ServiceResult.Failure(ExampleApiErrors.CreateInvalidRequestMissingWidgetIds());

			var widgets = await m_repository.GetWidgetBatchAsync(request.Ids, cancellationToken).ConfigureAwait(false);

			return ServiceResult.Success(new GetWidgetBatchResponseDto
			{
				Results = widgets.Select((x, i) =>
					x != null ? ServiceResult.Success(x) :
					string.IsNullOrEmpty(request.Ids[i]) ? ServiceResult.Failure(ExampleApiErrors.CreateInvalidRequestMissingWidgetId()) :
					ServiceResult.Failure(ExampleApiErrors.CreateNotFoundWidget(request.Ids[i]))).ToList()
			});
		}

		[Obsolete]
		public Task<ServiceResult<GetWidgetWeightResponseDto>> GetWidgetWeightAsync(GetWidgetWeightRequestDto request, CancellationToken cancellationToken)
		{
			return Task.FromResult<ServiceResult<GetWidgetWeightResponseDto>>(ServiceResult.Failure(ServiceErrors.CreateNotFound()));
		}

		public async Task<ServiceResult<GetPreferenceResponseDto>> GetPreferenceAsync(GetPreferenceRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrEmpty(request.Key))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest("Missing preference key."));

			var value = await m_repository.TryGetPreferenceAsync(request.Key, cancellationToken).ConfigureAwait(false);

			if (value == null)
				return ServiceResult.Failure(ServiceErrors.CreateNotFound("Preference key not found."));

			return ServiceResult.Success(new GetPreferenceResponseDto { Value = value });
		}

		public async Task<ServiceResult<SetPreferenceResponseDto>> SetPreferenceAsync(SetPreferenceRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrEmpty(request.Key))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest("Missing preference key."));

			await m_repository.SetPreferenceAsync(request.Key, request.Value, cancellationToken).ConfigureAwait(false);

			return ServiceResult.Success(new SetPreferenceResponseDto { Value = request.Value });
		}

		public Task<ServiceResult<GetInfoResponseDto>> GetInfoAsync(GetInfoRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return Task.FromResult(ServiceResult.Success(new GetInfoResponseDto { Name = "ExampleApi" }));
		}

		public Task<ServiceResult<NotRestfulResponseDto>> NotRestfulAsync(NotRestfulRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return Task.FromResult<ServiceResult<NotRestfulResponseDto>>(ServiceResult.Failure(ServiceErrors.CreateNotFound()));
		}

		public Task<ServiceResult<KitchenResponseDto>> KitchenAsync(KitchenRequestDto request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return Task.FromResult<ServiceResult<KitchenResponseDto>>(ServiceResult.Failure(ExampleApiErrors.CreateNotAdmin()));
		}

		public static string CreateWidgetETag(WidgetDto widget)
		{
			return $"W/\"{widget.Name}\"";
		}

		readonly IExampleApiRepository m_repository;
	}
}
