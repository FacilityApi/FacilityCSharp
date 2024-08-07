# ServiceHttpHandler.TryHandleServiceEventAsync&lt;TRequest,TResponse&gt; method

Attempts to handle a service method.

```csharp
protected Task<HttpResponseMessage?> TryHandleServiceEventAsync<TRequest, TResponse>(
    HttpMethodMapping<TRequest, TResponse> mapping, HttpRequestMessage httpRequest, 
    Func<TRequest, CancellationToken, Task<ServiceResult<IAsyncEnumerable<ServiceResult<TResponse>>>>> invokeEventAsync, 
    CancellationToken cancellationToken)
    where TRequest : ServiceDto, new()
    where TResponse : ServiceDto, new()
```

## See Also

* class [HttpMethodMapping&lt;TRequest,TResponse&gt;](../HttpMethodMapping-2.md)
* class [ServiceResult&lt;T&gt;](../../Facility.Core/ServiceResult-1.md)
* class [ServiceDto](../../Facility.Core/ServiceDto.md)
* class [ServiceHttpHandler](../ServiceHttpHandler.md)
* namespace [Facility.Core.Http](../../Facility.Core.md)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
