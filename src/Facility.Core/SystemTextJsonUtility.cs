using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Facility.Core;

internal static class SystemTextJsonUtility
{
	public static bool DeepEquals(JsonNode? first, JsonNode? second)
	{
		if (ReferenceEquals(first, second))
			return true;
		if (first is null && second is null)
			return true;

		if (first is JsonValue firstValue && second is JsonValue secondValue)
		{
			return DeepEquals(firstValue, secondValue);
		}

		if (first is JsonArray firstArray && second is JsonArray secondArray)
		{
			if (firstArray.Count != secondArray.Count)
				return false;
			for (var i = 0; i < firstArray.Count; i++)
			{
				if (!DeepEquals(firstArray[i], secondArray[i]))
					return false;
			}

			return true;
		}

		if (first is JsonObject firstObject && second is JsonObject secondObject)
		{
			if (firstObject.Count != secondObject.Count)
				return false;
			foreach (var prop in firstObject)
			{
				if (!secondObject.TryGetPropertyValue(prop.Key, out var secondObjectValue))
					return false;
				if (!DeepEquals(prop.Value, secondObjectValue))
					return false;
			}

			return true;
		}

		return false;
	}

	public static bool DeepEquals(JsonValue first, JsonValue second)
	{
		if (first.TryGetValue(out JsonElement firstElement) && second.TryGetValue(out JsonElement secondElement))
			return firstElement.IsEquivalentTo(secondElement);
		var firstBytes = JsonSerializer.SerializeToUtf8Bytes(first);
		var secondBytes = JsonSerializer.SerializeToUtf8Bytes(second);
		return StructuralComparisons.StructuralEqualityComparer.Equals(firstBytes, secondBytes);
	}

	public static bool DeepEquals(JsonElement first, JsonElement second) => first.IsEquivalentTo(second);

	/// copied from Json.More
	/// <summary>
	/// Determines JSON-compatible equivalence.
	/// </summary>
	/// <param name="a">The first element.</param>
	/// <param name="b">The second element.</param>
	/// <returns><code>true</code> if the element are equivalent; <code>false</code> otherwise.</returns>
	/// <exception cref="ArgumentOutOfRangeException">The <see cref="JsonElement.ValueKind"/> is not valid.</exception>
	public static bool IsEquivalentTo(this JsonElement a, JsonElement b)
	{
		if (a.ValueKind != b.ValueKind) return false;
		switch (a.ValueKind)
		{
			case JsonValueKind.Object:
				var aProperties = a.EnumerateObject().ToList();
				var bProperties = b.EnumerateObject().ToList();
				if (aProperties.Count != bProperties.Count) return false;
				var grouped = aProperties.Concat(bProperties)
					.GroupBy(p => p.Name)
					.Select(g => g.ToList())
					.ToList();
				return grouped.All(g => g.Count == 2 && g[0].Value.IsEquivalentTo(g[1].Value));
			case JsonValueKind.Array:
				var aElements = a.EnumerateArray().ToList();
				var bElements = b.EnumerateArray().ToList();
				if (aElements.Count != bElements.Count) return false;
				var zipped = aElements.Zip(bElements, (ae, be) => (ae, be));
				return zipped.All(p => p.ae.IsEquivalentTo(p.be));
			case JsonValueKind.String:
				return a.GetString() == b.GetString();
			case JsonValueKind.Number:
				return a.GetDecimal() == b.GetDecimal();
			case JsonValueKind.Undefined:
				return false;
			case JsonValueKind.True:
			case JsonValueKind.False:
			case JsonValueKind.Null:
				return true;
			default:
				throw new ArgumentOutOfRangeException(nameof(a));
		}
	}
}
