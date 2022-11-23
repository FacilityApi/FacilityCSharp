namespace Facility.Core;

internal interface IServiceNullable
{
	bool IsDefault { get; }
	object? Value { get; }
}
