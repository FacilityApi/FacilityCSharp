using BenchmarkDotNet.Attributes;
using Facility.AspNetCore;
using Facility.Benchmarks.Http;
using Facility.Core;
using Facility.Core.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Facility.Benchmarks;

[MinIterationCount(100), MaxIterationCount(10_000)]
[MemoryDiagnoser]
public class HttpBenchmarks
{
	[ParamsSource(nameof(Serializers))]
	public ServiceSerializer Serializer { get; set; } = default!;

	[Params(1, 10, 100, 1000)]
	public int UserCount { get; set; }

	public IReadOnlyList<ServiceSerializer> Serializers => new ServiceSerializer[]
	{
		//// NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
		//// ProtobufServiceSerializer.Instance,
		MessagePackServiceSerializer.Instance,
	};

	[GlobalSetup]
	public async Task Setup()
	{
		var baseUrl = "http://localhost:5987";
		m_webHost = new WebHostBuilder()
			.UseKestrel()
			.UseUrls(baseUrl)
			.ConfigureServices(services =>
			{
				services.AddSingleton(Serializer);
				services.AddSingleton<UserRepository>();
				services.AddSingleton<IBenchmarkService, BenchmarkService>();
				services.AddSingleton(x => new BenchmarkServiceHttpHandler(x.GetRequiredService<IBenchmarkService>(), new ServiceHttpHandlerSettings { ServiceSerializer = x.GetRequiredService<ServiceSerializer>() }));
				services.AddControllers(options =>
				{
					options.Filters.Add<FacilityActionFilter>();
				});
			})
			.Configure(app =>
			{
				app.UseRouting();
				app.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();
				});
			})
			.Build();

		await m_webHost.StartAsync();

		m_client = new HttpClientBenchmarkService(new HttpClientServiceSettings { BaseUri = new Uri(baseUrl), ServiceSerializer = Serializer });
	}

	[GlobalCleanup]
	public async Task Cleanup()
	{
		m_client = null;

		await m_webHost!.StopAsync();
		m_webHost.Dispose();
		m_webHost = null;
	}

	[Benchmark]
	public async Task<GetUsersResponseDto> GetUsers()
	{
		var result = await m_client!.GetUsersAsync(new() { Limit = UserCount });
		return result.Value;
	}

	private IWebHost? m_webHost;
	private HttpClientBenchmarkService? m_client;
}
