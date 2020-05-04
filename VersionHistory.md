# Version History

### 2.3.0

* Support DTO validation, specifically for `[required]` fields.

### 2.2.1

* Support nullable references.
* Use `--nullable` to generate code with nullable references.
* Add `ServiceResult.ToFailure`.

### 2.1.0

* Expose `BaseUri` to derived `HttpClientService`.
* Stop using `System.Net.Http` reference.
* Minor optimization in `HttpClientService`.

### 2.0.3

* Stop using `System.Net.Http` NuGet package.

### 2.0.2

* Upgrade to .NET Standard 2.0.
* Upgrade NuGet dependencies, including `Facility.Definition` and `Facility.CodeGen.Console`.
* Convert `fsdgencsharp` to a .NET Core Global Tool.
* Drop `--csproj` support from `fsdgencsharp` (SDK-style projects don't need it.)
* Support static `CSharpGenerator.GenerateCSharp` for C# build scripts.
* Publish `Facility.ConformanceApi` library and `FacilityConformance` tool.
* Allow null `ServiceHttpHandlerSettings`.

### 1.3.2

* Start tracking version history.
