# HttpContentSerializer.ReadHttpContentOrNullAsync method (1 of 2)

Reads an object from the specified HTTP content, or null if the content is missing or empty.

```csharp
public Task<ServiceResult<object?>> ReadHttpContentOrNullAsync(Type objectType, 
    HttpContent? content, CancellationToken cancellationToken = default)
```

## See Also

* class [ServiceResult&lt;T&gt;](../../Facility.Core/ServiceResult-1.md)
* class [HttpContentSerializer](../HttpContentSerializer.md)
* namespace [Facility.Core.Http](../../Facility.Core.md)

---

# HttpContentSerializer.ReadHttpContentOrNullAsync&lt;T&gt; method (2 of 2)

Reads a DTO from the specified HTTP content, or null if the content is missing or empty.

```csharp
public Task<ServiceResult<T?>> ReadHttpContentOrNullAsync<T>(HttpContent? content, 
    CancellationToken cancellationToken = default)
    where T : ServiceDto
```

## See Also

* class [ServiceResult&lt;T&gt;](../../Facility.Core/ServiceResult-1.md)
* class [ServiceDto](../../Facility.Core/ServiceDto.md)
* class [HttpContentSerializer](../HttpContentSerializer.md)
* namespace [Facility.Core.Http](../../Facility.Core.md)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
