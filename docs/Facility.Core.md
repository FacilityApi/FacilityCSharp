# Facility.Core assembly

## Facility.Core namespace

| public type | description |
| --- | --- |
| interface [IServiceMethodInfo](./Facility.Core/IServiceMethodInfo.md) | Information about a Facility service method. |
| abstract class [JsonServiceSerializer](./Facility.Core/JsonServiceSerializer.md) | Serializes and deserializes values to and from JSON. |
| class [NewtonsoftJsonServiceSerializer](./Facility.Core/NewtonsoftJsonServiceSerializer.md) | Serializes to and from JSON using JsonSerializer. |
| static class [ServiceDataUtility](./Facility.Core/ServiceDataUtility.md) | Helper methods for service data. |
| delegate [ServiceDelegator](./Facility.Core/ServiceDelegator.md) | Called when delegating a service method. |
| static class [ServiceDelegators](./Facility.Core/ServiceDelegators.md) | Common service delegators. |
| abstract class [ServiceDto&lt;T&gt;](./Facility.Core/ServiceDto-1.md) | Base class for data objects used by services. |
| abstract class [ServiceDto](./Facility.Core/ServiceDto.md) | Base class for data objects used by services. |
| abstract class [ServiceEnumJsonConverter&lt;T&gt;](./Facility.Core/ServiceEnumJsonConverter-1.md) | Used to JSON-serialize string-based enumerated types. |
| abstract class [ServiceEnumSystemTextJsonConverter&lt;T&gt;](./Facility.Core/ServiceEnumSystemTextJsonConverter-1.md) | Used to JSON-serialize string-based enumerated types. |
| class [ServiceErrorDto](./Facility.Core/ServiceErrorDto.md) | An error. |
| static class [ServiceErrors](./Facility.Core/ServiceErrors.md) | Common service errors. |
| static class [ServiceErrorUtility](./Facility.Core/ServiceErrorUtility.md) | Helper methods for service errors. |
| class [ServiceException](./Facility.Core/ServiceException.md) | An exception thrown by a service method. |
| abstract class [ServiceJsonConverterBase&lt;T&gt;](./Facility.Core/ServiceJsonConverterBase-1.md) | Base class for simple JSON converters. |
| static class [ServiceJsonUtility](./Facility.Core/ServiceJsonUtility.md) | Helper methods for working with Json.NET. |
| static class [ServiceMethodInfo](./Facility.Core/ServiceMethodInfo.md) | Helpers for service method information. |
| struct [ServiceNullable&lt;T&gt;](./Facility.Core/ServiceNullable-1.md) | Used to distinguish unspecified from null. |
| class [ServiceNullableDefaultValueAttribute](./Facility.Core/ServiceNullableDefaultValueAttribute.md) | Sets the `DefaultValue` to `new T()` for the specified type. |
| class [ServiceNullableNewtonsoftJsonConverter](./Facility.Core/ServiceNullableNewtonsoftJsonConverter.md) | Used by Json.NET to convert [`ServiceNullable`](./Facility.Core/ServiceNullable-1.md). |
| class [ServiceNullableSystemTextJsonConverter&lt;T&gt;](./Facility.Core/ServiceNullableSystemTextJsonConverter-1.md) | Used by `System.Text.Json` to convert [`ServiceNullable`](./Facility.Core/ServiceNullable-1.md). |
| class [ServiceNullableSystemTextJsonConverter](./Facility.Core/ServiceNullableSystemTextJsonConverter.md) | Used by `System.Text.Json` to convert [`ServiceNullable`](./Facility.Core/ServiceNullable-1.md). |
| class [ServiceObject](./Facility.Core/ServiceObject.md) | Encapsulates a JSON object. |
| class [ServiceObjectNewtonsoftJsonConverter](./Facility.Core/ServiceObjectNewtonsoftJsonConverter.md) | Used by Json.NET to convert [`ServiceObject`](./Facility.Core/ServiceObject.md). |
| class [ServiceObjectSystemTextJsonConverter](./Facility.Core/ServiceObjectSystemTextJsonConverter.md) | Used by `System.Text.Json` to convert [`ServiceObject`](./Facility.Core/ServiceObject.md). |
| class [ServiceResult&lt;T&gt;](./Facility.Core/ServiceResult-1.md) | A service result value or error. |
| class [ServiceResult](./Facility.Core/ServiceResult.md) | A service result success or error. |
| class [ServiceResultFailure](./Facility.Core/ServiceResultFailure.md) | A failed service result. |
| class [ServiceResultSystemTextJsonConverter&lt;TServiceResult&gt;](./Facility.Core/ServiceResultSystemTextJsonConverter-1.md) | Used by `System.Text.Json` to convert [`ServiceResult`](./Facility.Core/ServiceResult.md). |
| class [ServiceResultSystemTextJsonConverter](./Facility.Core/ServiceResultSystemTextJsonConverter.md) | Used by `System.Text.Json` to convert [`ServiceResult`](./Facility.Core/ServiceResult.md). |
| static class [ServiceResultUtility](./Facility.Core/ServiceResultUtility.md) | Helper methods for working with `ServiceResult`. |
| class [ServiceSerializationException](./Facility.Core/ServiceSerializationException.md) | Thrown when JSON deserialization fails. |
| abstract class [ServiceSerializer](./Facility.Core/ServiceSerializer.md) | Serializes and deserializes values. |
| class [SystemTextJsonContextServiceSerializer](./Facility.Core/SystemTextJsonContextServiceSerializer.md) | Serializes and deserializes values to and from JSON using a `System.Text.Json` serializer context. |
| class [SystemTextJsonServiceSerializer](./Facility.Core/SystemTextJsonServiceSerializer.md) | Serializes and deserializes values to and from JSON using `System.Text.Json`. |

## Facility.Core.Http namespace

| public type | description |
| --- | --- |
| class [BytesHttpContentSerializer](./Facility.Core.Http/BytesHttpContentSerializer.md) | Serializes and deserializes bytes for HTTP requests and responses. |
| static class [CommonClientAspects](./Facility.Core.Http/CommonClientAspects.md) | Common implementations of [`HttpClientServiceAspect`](./Facility.Core.Http/HttpClientServiceAspect.md). |
| abstract class [HttpClientService](./Facility.Core.Http/HttpClientService.md) | Used by HTTP clients. |
| abstract class [HttpClientServiceAspect](./Facility.Core.Http/HttpClientServiceAspect.md) | Used to provide common functionality to every HTTP client request. |
| class [HttpClientServiceDefaults](./Facility.Core.Http/HttpClientServiceDefaults.md) | Defaults for HTTP client services. |
| class [HttpClientServiceSettings](./Facility.Core.Http/HttpClientServiceSettings.md) | Settings for HTTP client services. |
| abstract class [HttpContentSerializer](./Facility.Core.Http/HttpContentSerializer.md) | Serializes and deserializes values for HTTP requests and responses. |
| class [HttpMethodMapping&lt;TRequest,TResponse&gt;](./Facility.Core.Http/HttpMethodMapping-2.md) | Defines the HTTP mapping for a service method. |
| class [HttpResponseMapping&lt;TResponse&gt;](./Facility.Core.Http/HttpResponseMapping-1.md) | Defines the HTTP mapping for a service method response. |
| static class [HttpServiceErrors](./Facility.Core.Http/HttpServiceErrors.md) | Common service errors. |
| static class [HttpServiceUtility](./Facility.Core.Http/HttpServiceUtility.md) | Utility methods for HTTP services. |
| class [ServiceHttpContext](./Facility.Core.Http/ServiceHttpContext.md) | The context for service HTTP handlers. |
| abstract class [ServiceHttpHandler](./Facility.Core.Http/ServiceHttpHandler.md) | A service HTTP handler. |
| abstract class [ServiceHttpHandlerAspect](./Facility.Core.Http/ServiceHttpHandlerAspect.md) | Used to provide common functionality to every HTTP service handler. |
| class [ServiceHttpHandlerDefaults](./Facility.Core.Http/ServiceHttpHandlerDefaults.md) | Defaults for service HTTP handlers. |
| class [ServiceHttpHandlerSettings](./Facility.Core.Http/ServiceHttpHandlerSettings.md) | Settings for service HTTP handlers. |
| class [TextHttpContentSerializer](./Facility.Core.Http/TextHttpContentSerializer.md) | Serializes and deserializes text for HTTP requests and responses. |

<!-- DO NOT EDIT: generated by xmldocmd for Facility.Core.dll -->
