using System.Threading;
using System.Threading.Tasks;
using Facility.Core;

namespace Facility.TestServerApi.Core
{
	public sealed class TestServerApiService : ITestServerApi
	{
		public async Task<ServiceResult<GetApiInfoResponseDto>> GetApiInfoAsync(GetApiInfoRequestDto request, CancellationToken cancellationToken)
		{
			return ServiceResult.Success(
				new GetApiInfoResponseDto
				{
					Service = "TestServerApi",
					Version = "0.1.0",
				});
		}
	}
}
