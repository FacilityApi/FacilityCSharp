# ServiceResult class

A service result success or error.

```csharp
public class ServiceResult
```

## Public Members

| name | description |
| --- | --- |
| static [Success](ServiceResult/Success.md)() | Creates a successful result. |
| [Error](ServiceResult/Error.md) { get; } | The error. |
| [IsFailure](ServiceResult/IsFailure.md) { get; } | True if the result has an error. |
| [IsSuccess](ServiceResult/IsSuccess.md) { get; } | True if the result has a value. |
| [AsFailure](ServiceResult/AsFailure.md)() | The service result as a failure; null if it is a success. |
| [Cast&lt;T&gt;](ServiceResult/Cast.md)() | Casts to a ServiceResult with a value. |
| [IsEquivalentTo](ServiceResult/IsEquivalentTo.md)(…) | Check service results for equivalence. |
| [ToFailure](ServiceResult/ToFailure.md)() | The service result as a failure; throws if it is a success. |
| override [ToString](ServiceResult/ToString.md)() | Render result as a string. |
| [Validate](ServiceResult/Validate.md)(…) | Validates the server result value. |
| [Verify](ServiceResult/Verify.md)() | Throws a ServiceException if the result is an error. |
| static [Failure](ServiceResult/Failure.md)(…) | Creates a failed result. |
| static [Success&lt;T&gt;](ServiceResult/Success.md)(…) | Creates a successful result. |
| class [ServiceResultJsonConverter](ServiceResult.ServiceResultJsonConverter.md) | Used by Json.NET to convert [`ServiceResult`](./ServiceResult.md). |

## See Also

* namespace [Facility.Core](../Facility.Core.md)
* [ServiceResult.cs](https://github.com/FacilityApi/FacilityCSharp/tree/master/src/Facility.Core/ServiceResult.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
