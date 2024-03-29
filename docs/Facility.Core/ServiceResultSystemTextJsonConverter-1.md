# ServiceResultSystemTextJsonConverter&lt;TServiceResult&gt; class

Used by `System.Text.Json` to convert [`ServiceResult`](./ServiceResult.md).

```csharp
public sealed class ServiceResultSystemTextJsonConverter<TServiceResult> : 
    JsonConverter<TServiceResult>
    where TServiceResult : ServiceResult
```

## Public Members

| name | description |
| --- | --- |
| [ServiceResultSystemTextJsonConverter](ServiceResultSystemTextJsonConverter-1/ServiceResultSystemTextJsonConverter.md)() | The default constructor. |
| override [Read](ServiceResultSystemTextJsonConverter-1/Read.md)(…) | Reads the JSON representation of the object. |
| override [Write](ServiceResultSystemTextJsonConverter-1/Write.md)(…) | Writes the JSON representation of the object. |

## See Also

* class [ServiceResult](./ServiceResult.md)
* namespace [Facility.Core](../Facility.Core.md)
* [ServiceResultSystemTextJsonConverter.cs](https://github.com/FacilityApi/FacilityCSharp/tree/master/src/Facility.Core/ServiceResultSystemTextJsonConverter.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
