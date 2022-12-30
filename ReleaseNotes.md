# Release Notes

## 2.18.0

* Update dependencies, including Newtonsoft.Json.
* Add .NET 7; remove .NET Core 3.1 and .NET 5.

## 2.17.0

* Support nullable fields.

## 2.16.3

* Add `--disable-chunked-transfer` to conformance app.
* Trim beginning slash from path in conformance tester.

## 2.16.2

* Don't escape "unsafe" punctuation and non-ASCII when writing JSON.
* Avoid broken default serialization when using JToken with System.Text.Json.

## 2.16.1

* Don't use wildcard media type from `Accept` header (e.g. `*/*` or `text/*`) directly as `Content-Type` of response.

## 2.16.0

* Support [MessagePack](https://github.com/neuecc/MessagePack-CSharp) for serialization.
  * To use faster/smaller indexed keys, add `[msgpack(key: 0)]`, `[msgpack(key: 1)]`, etc. to the data fields in your FSD.
  * Use `--msgpack` with `fsdgencsharp` to generate MessagePack-compatible DTOs.
  * Reference the `Facility.Core.MessagePack` NuGet package in your client and/or service.
  * For clients, set `HttpClientServiceSettings.ContentSerializer` to `MessagePackServiceSerializer.Instance` to send and receive MessagePack (rather than JSON) by specifying `application/msgpack` in the `Content-Type` and `Accept` headers.
  * For servers, set `ServiceHttpHandlerSettings.ContentSerializer` to `HttpContentSerializer.Combine(SystemTextJsonServiceSerializer.Instance, MessagePackServiceSerializer.Instance)` to support JSON by default but use MessagePack when `application/msgpack` is specified in the `Accept` header.
* Improve API for accessing or mutating service objects.
  * Allow service objects to be mutated via `AsJObject` or `AsJsonObject`. Add clarifying comments.
  * Add `ToJObject` and `ToJsonObject` for safer access.

## 2.15.2

* Add .NET 6 targets.

## 2.15.1

* Catch `UriFormatException` on request.

## 2.15.0

* Support fixing snake case in definitions.

## 2.14.6

* Improve performance of `NewtonsoftJsonServiceSerializer.Clone`.

## 2.14.5

* Fix build that didn't work properly on .NET 5 or .NET Core 3.1.

## 2.14.4

* Generated delegating service class as a partial class.

## 2.14.2–2.14.3

* Support `Newtonsoft.Json.Linq` properties with `SystemTextJsonServiceSerializer`.

## 2.14.1

* Use conformance data that serializes precisely.

## 2.14.0

* Serialize and deserialize JSON using `System.Text.Json` and async I/O.
  * Old clients and services will continue to use `Newtonsoft.Json` (Json.NET) until code is regenerated, after which `System.Text.Json` will be used unless `ContentSerializer` is set to `NewtonsoftJsonServiceSerializer.Instance`.
  * ASP.NET Core applications that continue to use Json.NET will need to set `AllowSynchronousIO` to true, since Json.NET doesn't support async serialization.
* Use chunked transfer encoding. Add `DisableChunkedTransfer` setting.
  * DTOs are no longer serialized to/from an intermediate memory stream, which should improve performance and memory usage on clients and servers.
* Use `ServiceObject` instead of `JObject` for FSD `object` fields. This is a **breaking change** in generated DTOs that use `object`.
* Support .NET 6.

## 2.13.5

* Add .NET 6 support to tools.

## 2.13.4

* Fix nullability of generated `Equals`.
* Drop .NET Core 2.1 support from tools.

## 2.13.3

* Fix bug when calling `IsDefined` on default enums.

## 2.13.2

* Don't do cache lookup when not needed.
* Add explicit visibility to code-generated fields.

## 2.13.1

* Fix bug when comparing default enums.

## 2.13.0

* Generate string constants for enums.

## 2.12.0

* Create standard validating delegator.

## 2.11.0

* Support DTO validation with `[validate]` fields.

## 2.10.0

* Fix bug where HTTP path was not case-insensitive when path variables were used.
* HTTP query parameter names should be case-insensitive.
* Add some conformance tests for case insensitivity.

## 2.9.0

* Support bytes/strings as request/response body fields.
* Support custom content type on request/response body fields.
* Support `Content-Type` as header fields.

## 2.8.1

* Update dependencies.

## 2.8.0

* Support and generate delegating implementations of services.
* Allow default cancellation token on public client methods.

## 2.7.2

* Add .NET 5 support to `fsdgencsharp` and `FacilityConformance`.
* Nullable reference improvement to `ServiceResult.GetValueOrDefault`.

## 2.7.1

* Simplify generated null check.

## 2.7.0

* Add .NET Standard 2.1 target.
* Make `ServiceResult<T>.GetValueOrDefault` maybe null.

## 2.6.0

* Only load JSON streams into memory when `ForceAsyncIO` is set.
* Add `JsonHttpContentSerializer.MemoryStreamCreator`.

## 2.5.0

* Load JSON request streams into memory to avoid synchronous I/O, which ASP.NET Core doesn't allow by default. (This was already true for JSON responses.)

## 2.4.1

* Add .NET Core App 3.1 support to `fsdgencsharp` and `FacilityConformance`.
* A few nullable reference improvements.
* Update `Facility.Definition` and `Facility.CodeGen.Console`.

## 2.4.0

* Support shorthand for required attribute, e.g. `string!`.

## 2.3.0

* Support DTO validation, specifically for `[required]` fields.

## 2.2.1

* Support nullable references.
* Use `--nullable` to generate code with nullable references.
* Add `ServiceResult.ToFailure`.

## 2.1.0

* Expose `BaseUri` to derived `HttpClientService`.
* Stop using `System.Net.Http` reference.
* Minor optimization in `HttpClientService`.

## 2.0.3

* Stop using `System.Net.Http` NuGet package.

## 2.0.2

* Upgrade to .NET Standard 2.0.
* Upgrade NuGet dependencies, including `Facility.Definition` and `Facility.CodeGen.Console`.
* Convert `fsdgencsharp` to a .NET Core Global Tool.
* Drop `--csproj` support from `fsdgencsharp` (SDK-style projects don't need it.)
* Support static `CSharpGenerator.GenerateCSharp` for C# build scripts.
* Publish `Facility.ConformanceApi` library and `FacilityConformance` tool.
* Allow null `ServiceHttpHandlerSettings`.

## 1.3.2

* Start tracking version history.
