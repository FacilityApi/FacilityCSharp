# Version History

## Pending

Describe changes here when they're committed to the `master` branch. To publish, update the version number in [Directory.Build.props](src/Directory.Build.props), move the pending changes below to a new [Released](#released) section, and push a git tag using the version number, e.g. `v2.3.4`.

Prefix the description of the change with `[major]`, `[minor]`, or `[patch]` in accordance with [Semantic Versioning](https://semver.org/).

* [major] Upgrade to .NET Standard 2.0.
* [major] Upgrade NuGet dependencies, including `Facility.Definition` and `Facility.CodeGen.Console`.
* [major] Convert `fsdgencsharp` to a .NET Core Global Tool.
* [major] Drop `--csproj` support from `fsdgencsharp` (SDK-style projects don't need it.)
* [minor] Support static `CSharpGenerator.GenerateCSharp` for C# build scripts.
* [major] Publish `Facility.ConformanceApi` library and `FacilityConformance` tool.

## Released

### 1.3.2

* Start tracking version history.
