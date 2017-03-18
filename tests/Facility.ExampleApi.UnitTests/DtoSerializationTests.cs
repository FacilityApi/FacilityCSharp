using System;
using System.Collections.Generic;
using Facility.Core;
using Facility.Core.Assertions;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	public class DtoSerializationTests
	{
		[Test]
		public void SerializeEmptyPreference()
		{
			SerializePreference(
				new PreferenceDto(),
				expectedJson: "{}");
		}

		[Test]
		public void SerializeBoolean()
		{
			SerializePreference(
				new PreferenceDto
				{
					IsBoolean = true,
				},
				expectedJson: "{\"boolean\":true}");
		}

		[Test]
		public void SerializeBooleans()
		{
			SerializePreference(
				new PreferenceDto
				{
					Booleans = new[] { true, false },
				});
		}

		[Test]
		public void SerializeDouble()
		{
			SerializePreference(
				new PreferenceDto
				{
					Double = 3.14,
				});
		}

		[Test]
		public void SerializeDoubles()
		{
			SerializePreference(
				new PreferenceDto
				{
					Doubles = new[] { 3.14, 1.414, double.MinValue, double.MaxValue, double.Epsilon },
				});
		}

		[Test]
		public void SerializeInteger()
		{
			SerializePreference(
				new PreferenceDto
				{
					Integer = 3,
				});
		}

		[Test]
		public void SerializeIntegers()
		{
			SerializePreference(
				new PreferenceDto
				{
					Integers = new[] { 3, -42, int.MaxValue, int.MinValue },
				});
		}

		[Test]
		public void SerializeString()
		{
			SerializePreference(
				new PreferenceDto
				{
					String = "hello",
				});
		}

		[Test]
		public void SerializeStrings()
		{
			SerializePreference(
				new PreferenceDto
				{
					Strings = new[] { "hello", "world", "" },
				});
		}

		[Test]
		public void SerializeBytes()
		{
			SerializePreference(
				new PreferenceDto
				{
					Bytes = new[] { (byte) 1, (byte) 2, (byte) 3, byte.MaxValue, byte.MinValue },
				});
		}

		[Test]
		public void SerializeByteses()
		{
			SerializePreference(
				new PreferenceDto
				{
					Byteses = new[] { new[] { (byte) 1, (byte) 2, (byte) 3 }, new[] { (byte) 4, (byte) 5, (byte) 6 } },
				});
		}

		[Test]
		public void SerializeWidgetField()
		{
			SerializePreference(
				new PreferenceDto
				{
					WidgetField = WidgetField.Name,
				});
		}

		[Test]
		public void SerializeWidgetFields()
		{
			SerializePreference(
				new PreferenceDto
				{
					WidgetFields = new[] { WidgetField.Name, WidgetField.Id, new WidgetField("future") },
				});
		}

		[Test]
		public void SerializeWidget()
		{
			SerializePreference(
				new PreferenceDto
				{
					Widget = new WidgetDto { Id = "id", Name = "name", Price = 1.23m },
				});
		}

		[Test]
		public void SerializeWidgets()
		{
			SerializePreference(
				new PreferenceDto
				{
					Widgets = new[] { new WidgetDto { Id = "id", Name = "name", Price = 1.23m }, new WidgetDto { Id = "id2", Name = "name2", Price = 2.34m } },
				});
		}

		[Test]
		public void SerializeResult()
		{
			SerializePreference(
				new PreferenceDto
				{
					Result = ServiceResult.Success(new WidgetDto { Id = "id", Name = "name", Price = 1.23m }),
				});
		}

		[Test]
		public void SerializeResults()
		{
			SerializePreference(
				new PreferenceDto
				{
					Results = new[] { ServiceResult.Success(new WidgetDto { Id = "id", Name = "name", Price = 1.23m }), ServiceResult.Failure(new ServiceErrorDto { Code = "Epic" }) },
				});
		}

		[Test]
		public void SerializeBigInteger()
		{
			SerializePreference(
				new PreferenceDto
				{
					BigInteger = (long) int.MaxValue + 1,
				});
		}

		[Test]
		public void SerializeBigIntegers()
		{
			SerializePreference(
				new PreferenceDto
				{
					BigIntegers = new[] { (long) int.MaxValue + 1, (long) int.MinValue - 1, long.MaxValue, long.MinValue },
				});
		}

		[Test]
		public void SerializeError()
		{
			SerializePreference(
				new PreferenceDto
				{
					Error = new ServiceErrorDto { Code = "Zero" },
				});
		}

		[Test]
		public void SerializeErrors()
		{
			SerializePreference(
				new PreferenceDto
				{
					Errors = new[] { new ServiceErrorDto { Code = "One" }, new ServiceErrorDto { Code = "Two" } },
				});
		}

		[Test]
		public void SerializeObject()
		{
			SerializePreference(
				new PreferenceDto
				{
					Object = new JObject { { "number", 3.14 }, { "nested", new JObject { { "hey", "you" } } } },
				});
		}

		[Test]
		public void SerializeObjects()
		{
			SerializePreference(
				new PreferenceDto
				{
					Objects = new[] { new JObject { { "number", 3.14 } }, new JObject { { "null", null } } },
				});
		}

		[Test]
		public void SerializeEverything()
		{
			SerializePreference(
				new PreferenceDto
				{
					IsBoolean = true,
					Booleans = new[] { true, false },
					Double = 3.14,
					Doubles = new[] { 3.14, 1.414, double.MinValue, double.MaxValue, double.Epsilon },
					Integer = 3,
					Integers = new[] { 3, -42, int.MaxValue, int.MinValue },
					String = "hello",
					Strings = new[] { "hello", "world", "" },
					Bytes = new[] { (byte) 1, (byte) 2, (byte) 3, byte.MaxValue, byte.MinValue },
					Byteses = new[] { new[] { (byte) 1, (byte) 2, (byte) 3 }, new[] { (byte) 4, (byte) 5, (byte) 6 } },
					WidgetField = WidgetField.Name,
					WidgetFields = new[] { WidgetField.Name, WidgetField.Id, new WidgetField("future") },
					Widget = new WidgetDto { Id = "id", Name = "name" },
					Widgets = new[] { new WidgetDto { Id = "id", Name = "name" }, new WidgetDto { Id = "id2", Name = "name2" } },
					Result = ServiceResult.Success(new WidgetDto { Id = "id", Name = "name" }),
					Results = new[] { ServiceResult.Success(new WidgetDto { Id = "id", Name = "name" }), ServiceResult.Failure(new ServiceErrorDto { Code = "Epic" }) },
					BigInteger = (long) int.MaxValue + 1,
					BigIntegers = new[] { (long) int.MaxValue + 1, (long) int.MinValue - 1, long.MaxValue, long.MinValue },
					Decimal = 1.23m,
					Decimals = new [] { Decimal.MaxValue, Decimal.MinValue, Decimal.MinusOne, Decimal.One, Decimal.Zero },
					Error = new ServiceErrorDto { Code = "Zero" },
					Errors = new[] { new ServiceErrorDto { Code = "One" }, new ServiceErrorDto { Code = "Two" } },
					Object = new JObject { { "number", 3.14 }, { "nested", new JObject { { "hey", "you" } } } },
					Objects = new[] { new JObject { { "number", 3.14 } }, new JObject { { "null", null } } },
				});
		}

		[Test]
		public void SerializeSimpleFields()
		{
			SerializePreference(
				new PreferenceDto
				{
					IsBoolean = true,
					Double = 3.14,
					Integer = 3,
					String = "hello",
					Bytes = new[] { (byte) 1, (byte) 2, (byte) 3 },
					WidgetField = WidgetField.Name,
					Widget = new WidgetDto { Id = "id", Name = "name" },
				});
		}

		[Test]
		public void SerializeArrayFields()
		{
			SerializePreference(
				new PreferenceDto
				{
					Booleans = new[] { true, false },
					Doubles = new[] { 3.14, 1.414 },
					Integers = new[] { 3, -42 },
					Strings = new[] { "hello", "world" },
					Byteses = new[] { new[] { (byte) 1, (byte) 2, (byte) 3 }, new[] { (byte) 4, (byte) 5, (byte) 6 } },
					WidgetFields = new[] { WidgetField.Name, WidgetField.Id },
					Widgets = new[] { new WidgetDto { Id = "id", Name = "name" }, new WidgetDto { Id = "id2", Name = "name2" } },
				});
		}

		[Test]
		public void SerializeResultSuccessField()
		{
			SerializePreference(
				new PreferenceDto
				{
					Result = ServiceResult.Success(new WidgetDto { Id = "id", Name = "name" })
				});
		}

		[Test]
		public void SerializeResultFailureField()
		{
			SerializePreference(
				new PreferenceDto
				{
					Result = ServiceResult.Failure(new ServiceErrorDto { Code = "TheCode" })
				});
		}

		[Test]
		public void SerializeResultsField()
		{
			SerializePreference(
				new PreferenceDto
				{
					Results = new[]
					{
						ServiceResult.Success(new WidgetDto { Id = "id", Name = "name" }),
						ServiceResult.Failure(new ServiceErrorDto { Code = "TheCode" }),
					}
				});
		}

		[Test]
		public void KitchenSinkEquivalence()
		{
			var innerArray = new[] { "bar", "baz" };

			var response1 = new KitchenRequestDto
			{
				Sink = new KitchenSinkDto
				{
					Matrix = new[] { new[] { new[] { 1, 2 }, new[] { 3, 4 } }, new[] { new[] { 5, 6 }, new[] { 7, 8 } } },
					Crazy = new[]
					{
						ServiceResult.Success<IReadOnlyList<IReadOnlyDictionary<string, IReadOnlyList<string>>>>(new[]
						{
							(IReadOnlyDictionary<string, IReadOnlyList<string>>) new Dictionary<string, IReadOnlyList<string>> { ["foo"] = innerArray },
						}),
					},
				},
			};

			var response2 = ServiceDataUtility.Clone(response1);
			ServiceDataUtility.AreEquivalentFieldValues(response1, response2).Should().BeTrue();

			innerArray[1] = "xyzzy";
			ServiceDataUtility.AreEquivalentFieldValues(response1, response2).Should().BeFalse();
		}

		private void SerializePreference(PreferenceDto dto, string expectedJson = null)
		{
			string json = ServiceJsonUtility.ToJson(dto);
			if (expectedJson != null)
				json.Should().Be(expectedJson);
			ServiceJsonUtility.FromJson<PreferenceDto>(json).Should().BeDto(dto);
		}
	}
}
