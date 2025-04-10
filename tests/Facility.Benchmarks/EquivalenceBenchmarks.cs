using System.Text.Json.Nodes;
using BenchmarkDotNet.Attributes;
using Facility.ConformanceApi;
using Facility.Core;
using Facility.Core.MessagePack;

namespace Facility.Benchmarks;

[MemoryDiagnoser]
public class EquivalenceBenchmarks
{
	public EquivalenceBenchmarks()
	{
		m_dto1 = new AnyDto
		{
			String = new string('!', 10000),
			Boolean = true,
			Float = 3.14f,
			Double = 3.14159,
			Int32 = 42,
			Int64 = 9876543210,
			Decimal = 123.456m,
			Datetime = DateTime.UtcNow,
			Bytes = [1, 2, 3, 4, 5],
			Object = new JsonObject { { "key", "value" } },
			Error = new ServiceErrorDto { Code = "test_error", Message = "Test error message" },
			Enum = Answer.Yes,
			Array = new AnyArrayDto { String = ["one", "two", "three"] },
			Map = new AnyMapDto { String = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } } },
		};
		m_dto2 = ServiceDataUtility.Clone(m_dto1);
	}

	[Benchmark]
	public bool CodeGenEquivalence() => m_dto1.IsEquivalentTo(m_dto2);

	[Benchmark]
	public bool ToStringEquivalence() => m_dto1.ToString() == m_dto2.ToString();

	[Benchmark]
	public bool NewtonsoftToStringEquivalence() => NewtonsoftJsonServiceSerializer.Instance.ToJson(m_dto1) == NewtonsoftJsonServiceSerializer.Instance.ToJson(m_dto2);

	[Benchmark]
	public bool NewtonsoftToObjectEquivalence() => NewtonsoftJsonServiceSerializer.Instance.ToServiceObject(m_dto1)!.IsEquivalentTo(NewtonsoftJsonServiceSerializer.Instance.ToServiceObject(m_dto2));

	[Benchmark]
	public bool NewtonsoftEquivalence() => NewtonsoftJsonServiceSerializer.Instance.AreEquivalent(m_dto1, m_dto2);

	[Benchmark]
	public bool SystemTextJsonToStringEquivalence() => SystemTextJsonServiceSerializer.Instance.ToJson(m_dto1) == SystemTextJsonServiceSerializer.Instance.ToJson(m_dto2);

	[Benchmark]
	public bool SystemTextJsonToObjectEquivalence() => SystemTextJsonServiceSerializer.Instance.ToServiceObject(m_dto1)!.IsEquivalentTo(SystemTextJsonServiceSerializer.Instance.ToServiceObject(m_dto2));

	[Benchmark]
	public bool SystemTextJsonEquivalence() => SystemTextJsonServiceSerializer.Instance.AreEquivalent(m_dto1, m_dto2);

	[Benchmark]
	public bool MessagePackEquivalence() => MessagePackServiceSerializer.Instance.AreEquivalent(m_dto1, m_dto2);

	private readonly AnyDto m_dto1;
	private readonly AnyDto m_dto2;
}
