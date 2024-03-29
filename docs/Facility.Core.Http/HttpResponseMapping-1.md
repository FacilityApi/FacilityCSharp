# HttpResponseMapping&lt;TResponse&gt; class

Defines the HTTP mapping for a service method response.

```csharp
public sealed class HttpResponseMapping<TResponse>
    where TResponse : ServiceDto, new()
```

## Public Members

| name | description |
| --- | --- |
| [ResponseBodyContentType](HttpResponseMapping-1/ResponseBodyContentType.md) { get; } | The content type of the response body, if any. |
| [ResponseBodyType](HttpResponseMapping-1/ResponseBodyType.md) { get; } | The type of the response body, if any. |
| [StatusCode](HttpResponseMapping-1/StatusCode.md) { get; } | The status code used by this mapping. |
| [CreateResponse](HttpResponseMapping-1/CreateResponse.md)(…) | Creates a response with an optional body. |
| [GetResponseBody](HttpResponseMapping-1/GetResponseBody.md)(…) | Extracts the HTTP response content body from the response. |
| [MatchesResponse](HttpResponseMapping-1/MatchesResponse.md)(…) | True if the response should result in this status code and body. |
| class [Builder&lt;TResponse&gt;](HttpResponseMapping-1.Builder-1.md) | Used to build instances of this class. |

## See Also

* class [ServiceDto](../Facility.Core/ServiceDto.md)
* namespace [Facility.Core.Http](../Facility.Core.md)
* [HttpResponseMapping.cs](https://github.com/FacilityApi/FacilityCSharp/tree/master/src/Facility.Core/Http/HttpResponseMapping.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
