using System.Collections.Generic;
using Facility.Core;

namespace Facility.ConformanceApi.Testing;

/// <summary>
/// Settings for <see cref="ConformanceApiService"/>.
/// </summary>
public sealed class ConformanceApiServiceSettings
{
	/// <summary>
	/// The tests.
	/// </summary>
	public IReadOnlyList<ConformanceTestInfo>? Tests { get; set; }

	/// <summary>
	/// The JSON serializer to use.
	/// </summary>
	public JsonServiceSerializer? JsonSerializer { get; set; }
}
