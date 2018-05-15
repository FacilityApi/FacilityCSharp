using System;
using System.Collections.Generic;
using System.Linq;
using Faithlife.Parsing;
using Faithlife.Parsing.Json;
using static System.FormattableString;

namespace Facility.ServerTesting
{
	public static class TestUtility
	{
		public static bool TryParseJson(string json, out object value)
		{
			var parseResult = JsonParsers.JsonValue.TryParse(json);
			if (parseResult.Success)
			{
				value = parseResult.Value;
				return true;
			}
			else
			{
				value = null;
				return true;
			}
		}

		public static string RenderJson(object value)
		{
			if (value is IEnumerable<KeyValuePair<string, object>> valueAsObject)
			{
				return "{" + string.Join(",", valueAsObject.Select(x => RenderJson(x))) + "}";
			}
			else if (value is KeyValuePair<string, object> valueAsProperty)
			{
				return RenderJson(valueAsProperty.Key) + ":" + RenderJson(valueAsProperty.Value);
			}
			else if (value is string valueAsString)
			{
				return "\"" + valueAsString + "\"";
			}
			else
			{
				return value != null ? Invariant($"{value}") : "null";
			}
		}

		public static bool IsJsonEquivalent(object actual, object expected, out string message)
		{
			message = GetJsonNotEquivalentMessage(actual, expected);
			return message == null;
		}

		private static string GetJsonNotEquivalentMessage(object actual, object expected, string path = "")
		{
			if (actual is int actualInt32)
				actual = (long) actualInt32;
			if (expected is int expectedInt32)
				expected = (long) expectedInt32;

			if (expected is IEnumerable<KeyValuePair<string, object>> expectedObject)
			{
				if (!(actual is IEnumerable<KeyValuePair<string, object>> actualObject))
					return $"{renderPath("")} was not an object";

				using (var actualIterator = actualObject.GetEnumerator())
				{
					foreach (var expectedPair in expectedObject.OrderBy(x => x.Key, StringComparer.Ordinal))
					{
						if (!actualIterator.MoveNext() || actualIterator.Current.Key != expectedPair.Key)
							return $"{renderPath("object")} missing property '{expectedPair.Key}'";
						GetJsonNotEquivalentMessage(actualIterator.Current.Value, expectedPair.Value, combinePath(path, expectedPair.Key));
					}

					if (actualIterator.MoveNext())
						return $"{renderPath("object")} has extra property '{actualIterator.Current.Key}'";
				}
			}
			else if (expected is Func<object, bool> predicate)
			{
				if (!predicate(actual))
					return Invariant($"{renderPath("")} was {getTypeName(actual)} '{actual}'");
			}
			else
			{
				if (!Equals(actual, expected))
					return Invariant($"{renderPath("")} was {getTypeName(actual)} '{actual}'; expected {getTypeName(expected)} {expected}");
			}

			return null;

			string getTypeName(object o)
			{
				if (o is IEnumerable<KeyValuePair<string, object>>)
					return "object";
				if (o is IReadOnlyList<object>)
					return "array";
				if (o is int || o is long || o is double)
					return "number";
				if (o is string)
					return "string";
				if (o is bool)
					return "boolean";
				if (o == null)
					return "null";
				throw new InvalidOperationException($"Unexpected type {o.GetType().FullName}");
			}

			string combinePath(string a, string b) => a.Length == 0 ? b : $"{a}.{b}";

			string renderPath(string t) => (t.Length == 0 ? "response" : $"response {t}") + (path.Length == 0 ? "" : $" at '{path}'");
		}
	}
}
