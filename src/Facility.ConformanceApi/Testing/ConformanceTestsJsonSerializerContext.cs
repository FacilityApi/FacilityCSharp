#if NET8_0_OR_GREATER
using System.Text.Json.Serialization;
using Facility.Core;

namespace Facility.ConformanceApi.Testing;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ServiceObject))]
[JsonSerializable(typeof(ConformanceTestsInfo))]
[JsonSerializable(typeof(ConformanceTestInfo))]
[JsonSerializable(typeof(ConformanceHttpRequestInfo))]
internal sealed partial class ConformanceTestsJsonSerializerContext : JsonSerializerContext
{
}
#endif
