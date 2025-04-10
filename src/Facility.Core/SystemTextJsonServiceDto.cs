namespace Facility.Core;

/// <summary>
/// Base class for data objects that use System.Text.Json for serialization.
/// </summary>
/// <remarks><see cref="ServiceDto{T}" /> uses Newtonsoft.Json by default for backward compatibility.</remarks>
public abstract class SystemTextJsonServiceDto<T> : ServiceDto<T>
	where T : ServiceDto<T>
{
	/// <inheritdoc />
	protected override JsonServiceSerializer JsonSerializer => SystemTextJsonServiceSerializer.Instance;
}
