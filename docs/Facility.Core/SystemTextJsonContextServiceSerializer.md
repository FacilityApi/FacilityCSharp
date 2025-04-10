# SystemTextJsonContextServiceSerializer class

Serializes and deserializes values to and from JSON using a `System.Text.Json` serializer context.

```csharp
public class SystemTextJsonContextServiceSerializer : JsonServiceSerializer
```

## Public Members

| name | description |
| --- | --- |
| [SystemTextJsonContextServiceSerializer](SystemTextJsonContextServiceSerializer/SystemTextJsonContextServiceSerializer.md)(…) | Creates an instance that uses the specified serializer context. |
| override [AreEquivalent](SystemTextJsonContextServiceSerializer/AreEquivalent.md)(…) | Checks two values for equality by comparing serialized representations. |
| override [Clone&lt;T&gt;](SystemTextJsonContextServiceSerializer/Clone.md)(…) | Clones a value by serializing and deserializing. |
| override [FromJson](SystemTextJsonContextServiceSerializer/FromJson.md)(…) | Deserializes a value from JSON. |
| override [FromJson&lt;T&gt;](SystemTextJsonContextServiceSerializer/FromJson.md)(…) | Deserializes a value from JSON. |
| override [FromServiceObject](SystemTextJsonContextServiceSerializer/FromServiceObject.md)(…) | Deserializes a value from a [`ServiceObject`](./ServiceObject.md) representation of JSON. |
| override [FromServiceObject&lt;T&gt;](SystemTextJsonContextServiceSerializer/FromServiceObject.md)(…) | Deserializes a value from a [`ServiceObject`](./ServiceObject.md) representation of JSON. |
| override [FromStreamAsync](SystemTextJsonContextServiceSerializer/FromStreamAsync.md)(…) | Deserializes a value from JSON. |
| override [ToJson](SystemTextJsonContextServiceSerializer/ToJson.md)(…) | Serializes a value to JSON. |
| override [ToServiceObject](SystemTextJsonContextServiceSerializer/ToServiceObject.md)(…) | Serializes a value to a [`ServiceObject`](./ServiceObject.md) representation of JSON. |
| override [ToStreamAsync](SystemTextJsonContextServiceSerializer/ToStreamAsync.md)(…) | Serializes a value to JSON. |

## Remarks

Only the types supported by the serializer context can be serialized or deserialized.

## See Also

* class [JsonServiceSerializer](./JsonServiceSerializer.md)
* namespace [Facility.Core](../Facility.Core.md)
* [SystemTextJsonContextServiceSerializer.cs](https://github.com/FacilityApi/FacilityCSharp/tree/master/src/Facility.Core/SystemTextJsonContextServiceSerializer.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
