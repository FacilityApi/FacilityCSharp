# ServiceException class

An exception thrown by a service method.

```csharp
public class ServiceException : Exception
```

## Public Members

| name | description |
| --- | --- |
| [ServiceException](ServiceException/ServiceException.md)(…) | Creates an exception from an error data object. (2 constructors) |
| [Error](ServiceException/Error.md) { get; } | The error. |
| override [Message](ServiceException/Message.md) { get; } | The message. |
| override [ToString](ServiceException/ToString.md)() | The exception type name, full error, inner exception, and stack trace. |

## See Also

* namespace [Facility.Core](../Facility.Core.md)
* [ServiceException.cs](https://github.com/FacilityApi/FacilityCSharp/tree/master/src/Facility.Core/ServiceException.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
