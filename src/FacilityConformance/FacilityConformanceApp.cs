using System.Net;
using Facility.ConformanceApi;
using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core;
using Facility.Core.Http;
using Facility.Core.MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace FacilityConformance;

internal sealed class FacilityConformanceApp
{
	public static async Task<int> Main(string[] args)
	{
		try
		{
			return await new FacilityConformanceApp().RunAsync(args);
		}
		catch (ArgsReaderException exception)
		{
			if (!string.IsNullOrWhiteSpace(exception.Message))
			{
				Console.WriteLine(exception.Message);
				Console.WriteLine();
			}

			const int columnWidth = 40;
			Console.WriteLine("Commands:");
			Console.WriteLine("  host [--url <url>]".PadRight(columnWidth) + "Hosts a conformance server");
			Console.WriteLine("  test [--url <url>] [<test> ...]".PadRight(columnWidth) + "Tests a conformance server");
			Console.WriteLine("  fsd [--output <path> [--verify]]".PadRight(columnWidth) + "Writes the Conformance API FSD");
			Console.WriteLine("  json [--output <path> [--verify]]".PadRight(columnWidth) + "Writes the conformance test data");
			return -1;
		}
		catch (Exception exception)
		{
			Console.WriteLine(exception.ToString());
			return -2;
		}
	}

	public FacilityConformanceApp()
	{
		using (var fsdTextReader = new StreamReader(GetType().Assembly.GetManifestResourceStream("FacilityConformance.ConformanceApi.fsd")!))
			m_fsdText = fsdTextReader.ReadToEnd();

		using (var testsJsonReader = new StreamReader(GetType().Assembly.GetManifestResourceStream("FacilityConformance.ConformanceTests.json")!))
			m_testsJson = testsJsonReader.ReadToEnd();
	}

	public async Task<int> RunAsync(IReadOnlyList<string> args)
	{
		const string defaultUrl = "http://localhost:4117/";

		var argsReader = new ArgsReader(args);
		if (argsReader.ReadFlag("?|h|help"))
			throw new ArgsReaderException("");

		var serializerName = argsReader.ReadOption("serializer")?.ToLowerInvariant();
		var serializers = serializerName?.Split(',').Select(GetServiceSerializer).ToArray() ?? s_defaultSerializers;
		var contentSerializer = HttpContentSerializer.Combine([.. serializers.Select(HttpContentSerializer.Create)]);

#pragma warning disable CS0618 // Type or member is obsolete
		if (serializerName is "obsoletejson")
			contentSerializer = new JsonHttpContentSerializer(new JsonHttpContentSerializerSettings { ForceAsyncIO = true });
#pragma warning restore CS0618 // Type or member is obsolete

		var jsonSerializer = serializers.OfType<JsonServiceSerializer>().FirstOrDefault() ?? NewtonsoftJsonServiceSerializer.Instance;
		var tests = ConformanceTestsInfo.FromJson(m_testsJson).Tests!;

		var command = argsReader.ReadArgument();

		if (command == "host")
		{
			var url = argsReader.ReadOption("url") ?? defaultUrl;
			argsReader.VerifyComplete();

			var service = new ConformanceApiService(
				new ConformanceApiServiceSettings
				{
					Tests = tests,
					JsonSerializer = jsonSerializer,
				});

			await new WebHostBuilder()
				.UseKestrel(options => options.AllowSynchronousIO = serializerName is "newtonsoftjson")
				.UseUrls(url)
				.Configure(app => app.Run(httpContext => HostAsync(httpContext, service, contentSerializer)))
				.Build()
				.RunAsync();

			return 0;
		}

		if (command == "test")
		{
			var baseUri = new Uri(argsReader.ReadOption("url") ?? defaultUrl);
			var disableChunkedTransfer = argsReader.ReadFlag("disable-chunked-transfer");
			var testNames = argsReader.ReadArguments();
			argsReader.VerifyComplete();

			var api = new HttpClientConformanceApi(
				new HttpClientServiceSettings
				{
					BaseUri = baseUri,
					ContentSerializer = contentSerializer,
					DisableChunkedTransfer = disableChunkedTransfer,
				});

			var tester = new ConformanceApiTester(
				new ConformanceApiTesterSettings
				{
					Tests = tests,
					Api = api,
					HttpClient = new HttpClient { BaseAddress = baseUri },
					JsonSerializer = jsonSerializer,
				});

			var results = new List<ConformanceTestResult>();

			if (testNames.Count == 0)
			{
				results.AddRange((await tester.RunAllTestsAsync()).Results);
			}
			else
			{
				foreach (var testName in testNames)
				{
					var testInfo = tests.SingleOrDefault(x => x.Test == testName);
					if (testInfo == null)
					{
						Console.WriteLine($"Test not found: {testName}");
						return -1;
					}

					results.Add(await tester.RunTestAsync(testInfo));
				}
			}

			var failureCount = 0;
			foreach (var result in results.Where(x => x.Status == ConformanceTestStatus.Fail))
			{
				Console.WriteLine($"{result.TestName} fail: {result.Message}");
				failureCount++;
			}

			Console.WriteLine($"{results.Count} tests: {results.Count - failureCount} passed, {failureCount} failed.");

			return failureCount == 0 ? 0 : 1;
		}

		if (command == "fsd")
		{
			var outputPath = argsReader.ReadOption("output");
			var shouldVerify = argsReader.ReadFlag("verify");
			argsReader.VerifyComplete();

			return WriteText(path: outputPath, contents: m_fsdText, shouldVerify: shouldVerify);
		}

		if (command == "json")
		{
			var outputPath = argsReader.ReadOption("output");
			var shouldVerify = argsReader.ReadFlag("verify");
			argsReader.VerifyComplete();

			return WriteText(path: outputPath, contents: m_testsJson, shouldVerify: shouldVerify);
		}

		if (command != null)
			throw new ArgsReaderException($"Invalid command: {command}");

		throw new ArgsReaderException("Missing command.");
	}

	private static ServiceSerializer GetServiceSerializer(string serializerName) =>
		serializerName switch
		{
			"systemtextjson" => SystemTextJsonServiceSerializer.Instance,
			"newtonsoftjson" or "obsoletejson" => NewtonsoftJsonServiceSerializer.Instance,
			"messagepack" => MessagePackServiceSerializer.Instance,
			_ => throw new ArgsReaderException($"Unsupported serializer: {serializerName}"),
		};

	private static int WriteText(string? path, string contents, bool shouldVerify)
	{
		if (path != null)
		{
			if (shouldVerify)
			{
				if (!File.Exists(path: path) || File.ReadAllText(path: path) != contents)
					return 1;
			}
			else
			{
				var directory = Path.GetDirectoryName(path);
				if (directory != null && !Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				File.WriteAllText(path: path, contents: contents);
			}
		}
		else
		{
			if (shouldVerify)
				throw new ArgsReaderException("--verify requires --output.");

			Console.Write(contents);
		}

		return 0;
	}

	private async Task HostAsync(HttpContext httpContext, IConformanceApi service, HttpContentSerializer contentSerializer)
	{
		var httpRequest = httpContext.Request;
		var requestUrl = httpRequest.GetEncodedUrl();

		var apiHandler = new ConformanceApiHttpHandler(service, new ServiceHttpHandlerSettings { ContentSerializer = contentSerializer });

		var requestMessage = new HttpRequestMessage(new HttpMethod(httpRequest.Method), requestUrl)
		{
			Content = new StreamContent(httpRequest.Body),
		};

		foreach (var header in httpRequest.Headers)
		{
			// Every header should be able to fit into one of the two header collections.
			// Try message.Headers first since that accepts more of them.
			if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable()))
				requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.AsEnumerable());
		}

		HttpResponseMessage? responseMessage = null;
		ServiceErrorDto? error = null;

		try
		{
			responseMessage = await apiHandler.TryHandleHttpRequestAsync(requestMessage, httpContext.RequestAborted).ConfigureAwait(false);

			if (responseMessage == null)
				error = ServiceErrors.CreateInvalidRequest($"Test not found for {httpRequest.Method} {requestUrl}");
		}
		catch (Exception exception)
		{
			error = ServiceErrorUtility.CreateInternalErrorForException(exception);
		}

		if (error != null)
		{
			var statusCode = HttpServiceErrors.TryGetHttpStatusCode(error.Code) ?? HttpStatusCode.InternalServerError;
			responseMessage = new HttpResponseMessage(statusCode) { Content = contentSerializer.CreateHttpContent(error) };
		}

		if (responseMessage != null)
		{
			using (responseMessage)
			{
				var response = httpContext.Response;
				response.StatusCode = (int) responseMessage.StatusCode;

				var responseHeaders = responseMessage.Headers;

				// Ignore the Transfer-Encoding header if it is just "chunked".
				// We let the host decide about whether the response should be chunked or not.
				if (responseHeaders.TransferEncodingChunked == true && responseHeaders.TransferEncoding.Count == 1)
					responseHeaders.TransferEncoding.Clear();

				foreach (var header in responseHeaders)
					response.Headers.Append(header.Key, header.Value.ToArray());

				var contentHeaders = responseMessage.Content.Headers;

				// Copy the response content headers only after ensuring they are complete.
				// We ask for Content-Length first because HttpContent lazily computes this
				// and only afterwards writes the value into the content headers.
				_ = contentHeaders.ContentLength;

				foreach (var header in contentHeaders)
					response.Headers.Append(header.Key, header.Value.ToArray());

				await responseMessage.Content.CopyToAsync(response.Body).ConfigureAwait(false);
			}
		}
	}

	private static readonly IReadOnlyList<ServiceSerializer> s_defaultSerializers =
	[
		SystemTextJsonServiceSerializer.Instance,
		MessagePackServiceSerializer.Instance,
	];

	private readonly string m_fsdText;
	private readonly string m_testsJson;
}
