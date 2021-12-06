using Facility.Core;

namespace Facility.Benchmarks;

public sealed class BenchmarkService : IBenchmarkService
{
	public BenchmarkService(UserRepository userRepository)
	{
		m_userRepository = userRepository;
	}

	public async Task<ServiceResult<GetUsersResponseDto>> GetUsersAsync(GetUsersRequestDto request, CancellationToken cancellationToken = default)
	{
		var users = await m_userRepository.GetUsersAsync();
		return ServiceResult.Success(new GetUsersResponseDto { Items = users.Take(request.Limit ?? 10).ToList() });
	}

	private readonly UserRepository m_userRepository;
}
