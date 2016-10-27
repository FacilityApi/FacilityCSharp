using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;

namespace Facility.ExampleApi
{
	public static class ExampleApiUtility
	{
		public static Task<ServiceResult<GetWidgetsResponseDto>> GetWidgetsAsync(this IExampleApi service, string query = null, int? limit = null, WidgetField? sort = null, bool? desc = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = new GetWidgetsRequestDto { Query = query, Limit = limit, Sort = sort, Desc = desc };
			return service.GetWidgetsAsync(request, cancellationToken);
		}

		public static Task<ServiceResult<CreateWidgetResponseDto>> CreateWidgetAsync(this IExampleApi service, WidgetDto widget, CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = new CreateWidgetRequestDto { Widget = widget };
			return service.CreateWidgetAsync(request, cancellationToken);
		}

		public static Task<ServiceResult<GetWidgetResponseDto>> GetWidgetAsync(this IExampleApi service, string id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = new GetWidgetRequestDto { Id = id };
			return service.GetWidgetAsync(request, cancellationToken);
		}

		public static Task<ServiceResult<DeleteWidgetResponseDto>> DeleteWidgetAsync(this IExampleApi service, string id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = new DeleteWidgetRequestDto { Id = id };
			return service.DeleteWidgetAsync(request, cancellationToken);
		}

		public static Task<ServiceResult<GetWidgetBatchResponseDto>> GetWidgetBatchAsync(this IExampleApi service, IEnumerable<string> ids, CancellationToken cancellationToken = default(CancellationToken))
		{
			var request = new GetWidgetBatchRequestDto { Ids = ids?.ToList() };
			return service.GetWidgetBatchAsync(request, cancellationToken);
		}
	}
}
