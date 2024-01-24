using System.Collections;
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests.Http;

public class HttpClientServiceSettingsTests
{
	[Test]
	public async Task CloneSettings()
	{
		var original = new HttpClientServiceSettings
		{
			BaseUri = new Uri("https://example.com"),
			HttpClient = new HttpClient(),
			ContentSerializer = HttpContentSerializer.Create(SystemTextJsonServiceSerializer.Instance),
			BytesSerializer = BytesHttpContentSerializer.Instance,
			TextSerializer = TextHttpContentSerializer.Instance,
			DisableChunkedTransfer = true,
			Aspects = Array.Empty<HttpClientServiceAspect>(),
			Synchronous = true,
			SkipRequestValidation = true,
			SkipResponseValidation = true,
		};

		var clone = original.Clone();

		foreach (var property in typeof(HttpClientServiceSettings).GetProperties())
		{
			var propertyType = property.PropertyType;

			var originalValue = property.GetValue(original);
			var nullOrDefault = propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) is null ? Activator.CreateInstance(propertyType) : null;
			originalValue.Should().NotBe(nullOrDefault, $"original {property.Name} should not be null/default.");

			var clonedValue = property.GetValue(clone);
			if (clonedValue is IEnumerable and not string)
				clonedValue.Should().BeEquivalentTo(originalValue, $"cloned {property.Name} should be equivalent to original.");
			else
				clonedValue.Should().Be(originalValue, $"cloned {property.Name} should be equal to original.");
		}
	}
}
