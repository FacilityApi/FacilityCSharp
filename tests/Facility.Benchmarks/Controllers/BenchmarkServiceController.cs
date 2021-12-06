using Facility.Benchmarks.Http;
using Microsoft.AspNetCore.Components;

namespace Facility.Benchmarks.Controllers;

[Route("")]
public partial class BenchmarkServiceController
{
	public BenchmarkServiceController(BenchmarkServiceHttpHandler handler)
	{
		m_handler = handler;
	}

	private BenchmarkServiceHttpHandler GetServiceHttpHandler() => m_handler;

	private readonly BenchmarkServiceHttpHandler m_handler;
}
