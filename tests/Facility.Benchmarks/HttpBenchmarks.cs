using BenchmarkDotNet.Attributes;
using Facility.AspNetCore;
using Facility.Benchmarks.Http;
using Facility.Core;
using Facility.Core.Http;
using Facility.Core.MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Facility.Benchmarks;

[MinIterationCount(100), MaxIterationCount(10_000)]
[MemoryDiagnoser]
public class HttpBenchmarks
{
	[ParamsSource(nameof(Serializers))]
	public SerializerInfo Serializer { get; set; } = default!;

	[Params(1, 10, 100, 1000)]
	public int UserCount { get; set; }

	public IReadOnlyList<SerializerInfo> Serializers =>
	[
		new SerializerInfo(NewtonsoftJsonServiceSerializer.Instance),
		new SerializerInfo(SystemTextJsonServiceSerializer.Instance),
		new SerializerInfo(MessagePackServiceSerializer.Instance),
	];

	[GlobalSetup]
	public async Task Setup()
	{
		var baseUrl = "http://localhost:5987";
		m_webHost = new WebHostBuilder()
			.UseKestrel(options => options.AllowSynchronousIO = true)
			.UseUrls(baseUrl)
			.ConfigureServices(services =>
			{
				services.AddSingleton(Serializer.ServiceSerializer);
				services.AddSingleton<UserRepository>();
				services.AddSingleton<IBenchmarkService, BenchmarkService>();
				services.AddSingleton(x => new BenchmarkServiceHttpHandler(x.GetRequiredService<IBenchmarkService>(), new ServiceHttpHandlerSettings { ContentSerializer = HttpContentSerializer.Create(x.GetRequiredService<ServiceSerializer>()) }));
				services.AddControllers(options =>
				{
					options.Filters.Add<FacilityActionFilter>();
				});
			})
			.Configure(app =>
			{
				app.UseMiddleware<FacilityAspNetCoreMiddleware<BenchmarkServiceHttpHandler>>();
			})
			.Build();

		await m_webHost.StartAsync();

		m_client = new HttpClientBenchmarkService(
			new HttpClientServiceSettings
			{
				BaseUri = new Uri(baseUrl),
				ContentSerializer = HttpContentSerializer.Create(Serializer.ServiceSerializer),
			});
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
