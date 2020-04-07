# CSharpGenerator class

Generates C#.

```csharp
public sealed class CSharpGenerator : CodeGenerator
```

## Public Members

| name | description |
| --- | --- |
| [CSharpGenerator](CSharpGenerator/CSharpGenerator.md)() | The default constructor. |
| override [HasPatternsToClean](CSharpGenerator/HasPatternsToClean.md) { get; } | Patterns to clean are returned with the output. |
| [NamespaceName](CSharpGenerator/NamespaceName.md) { get; set; } | The name of the namespace (optional). |
| override [ApplySettings](CSharpGenerator/ApplySettings.md)(…) | Applies generator-specific settings. |
| override [GenerateOutput](CSharpGenerator/GenerateOutput.md)(…) | Generates the C# output. |
| static [GenerateCSharp](CSharpGenerator/GenerateCSharp.md)(…) | Generates C#. |

## See Also

* namespace [Facility.CodeGen.CSharp](../Facility.CodeGen.CSharp.md)
* [CSharpGenerator.cs](https://github.com/FacilityApi/FacilityCSharp/tree/master/src/Facility.CodeGen.CSharp/CSharpGenerator.cs)

<!-- DO NOT EDIT: generated by xmldocmd for Facility.CodeGen.CSharp.dll -->