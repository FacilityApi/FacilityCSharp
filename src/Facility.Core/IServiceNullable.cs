namespace Facility.Core;

internal interface IServiceNullable
{
	bool IsUnspecified { get; }
	object? Value { get; }
}
