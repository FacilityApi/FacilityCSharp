# ServiceDelegate.InvokeEventAsync method

Delegates the service event.

```csharp
public virtual Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>> InvokeEventAsync(
    IServiceEventInfo eventInfo, ServiceDto request, CancellationToken cancellationToken = default)
```

## See Also

* class [ServiceResult&lt;T&gt;](../ServiceResult-1.md)
* class [ServiceDto](../ServiceDto.md)
* interface [IServiceEventInfo](../IServiceEventInfo.md)
* class [ServiceDelegate](../ServiceDelegate.md)
* namespace [Facility.Core](../../Facility.Core.md)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
