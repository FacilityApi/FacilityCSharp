# ServiceResult&lt;T&gt;.Map&lt;TOutput&gt; method (1 of 2)

Maps a ServiceResult from one type to another.

```csharp
public ServiceResult<TOutput> Map<TOutput>(Func<T, ServiceResult<TOutput>> func)
```

## Remarks

If the result is a success, the function is called on the input value to produce a service result matching the type of the output value. If the result is a failure, the function is not called, and a failed service result using the output type is returned.

## See Also

* class [ServiceResult&lt;T&gt;](../ServiceResult-1.md)
* namespace [Facility.Core](../../Facility.Core.md)

---

# ServiceResult&lt;T&gt;.Map&lt;TOutput&gt; method (2 of 2)

Maps a ServiceResult from one type to another.

```csharp
public ServiceResult<TOutput> Map<TOutput>(Func<T, TOutput> func)
```

## Remarks

If the result is a success, the function is called on the input value to produce a successful service result matching the type of the output value. If the result is a failure, the function is not called, and a failed service result using the output type is returned.

## See Also

* class [ServiceResult&lt;T&gt;](../ServiceResult-1.md)
* namespace [Facility.Core](../../Facility.Core.md)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
