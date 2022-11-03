namespace Facility.Core;

internal interface IServiceField
{
	bool IsDefault { get; }
	object? Value { get; }
}
