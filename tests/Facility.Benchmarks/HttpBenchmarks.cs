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
internal sealed class HttpBenchmarks
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
		new SerializerInfo(BenchmarkServiceJsonServiceSerializer.Instance),
	];

	[GlobalSetup]
	public async Task Setup()
	{
		var baseUrl = "http://localhost:5987";
		var webAppBuilder = WebApplication.CreateSlimBuilder();
		webAppBuilder.WebHost.ConfigureKestrel(options => options.AllowSynchronousIO = true);
		webAppBuilder.WebHost.UseUrls(baseUrl);
		webAppBuilder.Services.AddSingleton(Serializer.ServiceSerializer);
		webAppBuilder.Services.AddSingleton<UserRepository>();
		webAppBuilder.Services.AddSingleton<IBenchmarkService, BenchmarkService>();
		webAppBuilder.Services.AddSingleton(x => new BenchmarkServiceHttpHandler(x.GetRequiredService<IBenchmarkService>(), new ServiceHttpHandlerSettings { ContentSerializer = HttpContentSerializer.Create(x.GetRequiredService<ServiceSerializer>()) }));
		webAppBuilder.Services.AddControllers(options =>
		{
			options.Filters.Add<FacilityActionFilter>();
		});
		m_webApp = webAppBuilder.Build();
		m_webApp.UseMiddleware<FacilityAspNetCoreMiddleware<BenchmarkServiceHttpHandler>>();

		await m_webApp.StartAsync();

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

		await m_webApp!.StopAsync();
		await m_webApp.DisposeAsync();
		m_webApp = null;
	}

	[Benchmark]
	public async Task<GetUsersResponseDto> GetUsers()
	{
		var result = await m_client!.GetUsersAsync(new() { Limit = UserCount });
		return result.Value;
	}

	private WebApplication? m_webApp;
	private HttpClientBenchmarkService? m_client;
}
