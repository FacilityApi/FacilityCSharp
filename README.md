# Facility C# Support

[C# support](https://facilityapi.github.io/generate/csharp) for the [Facility API Framework](https://facilityapi.github.io/).

[![Build](https://github.com/FacilityApi/FacilityCSharp/workflows/Build/badge.svg)](https://github.com/FacilityApi/FacilityCSharp/actions?query=workflow%3ABuild)

Name | Description | NuGet
--- | --- | ---
Facility.Core | A library for consuming/implementing Facility APIs. | [![NuGet](https://img.shields.io/nuget/v/Facility.Core.svg)](https://www.nuget.org/packages/Facility.Core)
Facility.Core.Assertions | FluentAssertions extensions for Facility unit tests. | [![NuGet](https://img.shields.io/nuget/v/Facility.Core.Assertions.svg)](https://www.nuget.org/packages/Facility.Core.Assertions)
Facility.Core.MessagePack | MesssagePack support for Facility. | [![NuGet](https://img.shields.io/nuget/v/Facility.Core.MessagePack.svg)](https://www.nuget.org/packages/Facility.Core.MessagePack)
fsdgencsharp | A tool that generates C# for a Facility Service Definition. | [![NuGet](https://img.shields.io/nuget/v/fsdgencsharp.svg)](https://www.nuget.org/packages/fsdgencsharp)
Facility.CodeGen.CSharp | A library that generates C# for a Facility Service Definition. | [![NuGet](https://img.shields.io/nuget/v/Facility.CodeGen.CSharp.svg)](https://www.nuget.org/packages/Facility.CodeGen.CSharp)
FacilityConformance | A tool that checks Facility conformance. | [![NuGet](https://img.shields.io/nuget/v/FacilityConformance.svg)](https://www.nuget.org/packages/FacilityConformance)
Facility.ConformanceApi | A .NET client for the standard Facility test server. | [![NuGet](https://img.shields.io/nuget/v/Facility.ConformanceApi.svg)](https://www.nuget.org/packages/Facility.ConformanceApi)

[Documentation](https://facilityapi.github.io/) | [Release Notes](https://github.com/FacilityApi/FacilityCSharp/blob/master/ReleaseNotes.md) | [Contributing](https://github.com/FacilityApi/FacilityCSharp/blob/master/CONTRIBUTING.md)

## Conformance

To run conformance tests, first start the conformance server:

```powershell
dotnet run --project .\src\FacilityConformance --framework net6.0 -- host
```

Then run the conformance tool against the running service.

```powershell
dotnet run --project .\src\FacilityConformance --framework net6.0 -- test
```

The <c>System.Text.Json</c> serializer is used by default. To use Json.NET or MessagePack, run with `--serializer newtonsoftjson` or  `--serializer messagepack`.
