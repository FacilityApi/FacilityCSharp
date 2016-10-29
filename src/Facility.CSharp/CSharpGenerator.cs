using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Http;

namespace Facility.CSharp
{
	/// <summary>
	/// Generates C#.
	/// </summary>
	public sealed class CSharpGenerator
	{
		/// <summary>
		/// The name of the generator for comments.
		/// </summary>
		public string GeneratorName { get; set; }

		/// <summary>
		/// The name of the namespace (optional).
		/// </summary>
		public string NamespaceName { get; set; }

		/// <summary>
		/// Generates the C# output.
		/// </summary>
		public IReadOnlyList<ServiceTextSource> GenerateOutput(ServiceDefinitionInfo definition)
		{
			var outputFiles = new List<ServiceTextSource>();

			var serviceInfo = definition.Service;

			var context = new Context
			{
				GeneratorName = GeneratorName,
				NamespaceName = NamespaceName ?? CSharpUtility.GetNamespaceName(serviceInfo),
				Service = serviceInfo,
			};

			foreach (var errorSetInfo in serviceInfo.ErrorSets)
				outputFiles.Add(GenerateErrorSet(errorSetInfo, context));

			foreach (var enumInfo in serviceInfo.Enums)
				outputFiles.Add(GenerateEnum(enumInfo, context));

			foreach (var dtoInfo in serviceInfo.Dtos)
				outputFiles.Add(GenerateDto(dtoInfo, context));

			if (serviceInfo.Methods.Count != 0)
			{
				outputFiles.Add(GenerateInterface(serviceInfo, context));

				foreach (var methodInfo in serviceInfo.Methods)
					outputFiles.AddRange(GenerateMethodDtos(methodInfo, context));
			}

			var httpServiceInfo = new HttpServiceInfo(serviceInfo);

			foreach (var httpErrorSetInfo in httpServiceInfo.ErrorSets)
				outputFiles.Add(GenerateHttpErrors(httpErrorSetInfo, context));

			if (httpServiceInfo.Methods.Count != 0)
			{
				outputFiles.Add(GenerateHttpMapping(httpServiceInfo, context));
				outputFiles.Add(GenerateHttpClient(httpServiceInfo, context));
				outputFiles.Add(GenerateHttpHandler(httpServiceInfo, context));
			}

			return outputFiles;
		}

		private ServiceTextSource GenerateErrorSet(ServiceErrorSetInfo errorSetInfo, Context context)
		{
			string fullErrorSetName = CSharpUtility.GetErrorSetName(errorSetInfo);

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);

				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				var usings = new List<string>
				{
					"Facility.Core",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

				if (!errorSetInfo.IsObsolete() && errorSetInfo.Errors.Any(x => x.IsObsolete()))
				{
					CSharpUtility.WriteObsoletePragma(code);
					code.WriteLine();
				}

				code.WriteLine($"namespace {context.NamespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, errorSetInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, errorSetInfo);

					code.WriteLine($"public static partial class {fullErrorSetName}");
					using (code.Block())
					{
						foreach (var errorInfo in errorSetInfo.Errors)
						{
							string errorsValue = errorInfo.Name;
							string memberName = CSharpUtility.GetErrorName(errorInfo);

							code.WriteLineSkipOnce();
							CSharpUtility.WriteSummary(code, errorInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, errorInfo);
							code.WriteLine($"public const string {memberName} = \"{errorsValue}\";");
						}

						foreach (var errorInfo in errorSetInfo.Errors)
						{
							code.WriteLine();
							string memberName = CSharpUtility.GetErrorName(errorInfo);
							CSharpUtility.WriteSummary(code, errorInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, errorInfo);
							code.WriteLine($"public static ServiceErrorDto Create{memberName}(string message = null)");
							using (code.Block())
								code.WriteLine($"return new ServiceErrorDto({memberName}, message ?? {CSharpUtility.CreateString(errorInfo.Summary)});");
						}
					}
				}

				return new ServiceTextSource(name: fullErrorSetName + CSharpUtility.FileExtension, text: stringWriter.ToString());
			}
		}

		private ServiceTextSource GenerateEnum(ServiceEnumInfo enumInfo, Context context)
		{
			string enumName = CSharpUtility.GetEnumName(enumInfo);

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);

				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				var usings = new List<string>
				{
					"System",
					"Facility.Core",
					"Newtonsoft.Json",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

				if (!enumInfo.IsObsolete() && enumInfo.Values.Any(x => x.IsObsolete()))
				{
					CSharpUtility.WriteObsoletePragma(code);
					code.WriteLine();
				}

				code.WriteLine($"namespace {context.NamespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, enumInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, enumInfo);

					code.WriteLine($"[JsonConverter(typeof({enumName}JsonConverter))]");
					code.WriteLine($"public struct {enumName} : IEquatable<{enumName}>");
					using (code.Block())
					{
						foreach (var enumValue in enumInfo.Values)
						{
							string memberName = CSharpUtility.GetEnumValueName(enumValue);

							code.WriteLineSkipOnce();
							CSharpUtility.WriteSummary(code, enumValue.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, enumValue);
							code.WriteLine($"public static readonly {enumName} {memberName} = new {enumName}(\"{enumValue.Name}\");");
						}

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Creates an instance.");
						code.WriteLine($"public {enumName}(string value)");
						using (code.Block())
							code.WriteLine("m_value = value;");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Converts the instance to a string.");
						code.WriteLine("public override string ToString()");
						using (code.Block())
							code.WriteLine("return m_value ?? \"\";");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for equality.");
						code.WriteLine($"public bool Equals({enumName} other)");
						using (code.Block())
							code.WriteLine("return StringComparer.OrdinalIgnoreCase.Equals(ToString(), other.ToString());");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for equality.");
						code.WriteLine("public override bool Equals(object obj)");
						using (code.Block())
							code.WriteLine($"return obj is {enumName} && Equals(({enumName}) obj);");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Gets the hash code.");
						code.WriteLine("public override int GetHashCode()");
						using (code.Block())
							code.WriteLine("return StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for equality.");
						code.WriteLine($"public static bool operator ==({enumName} left, {enumName} right)");
						using (code.Block())
							code.WriteLine("return left.Equals(right);");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for inequality.");
						code.WriteLine($"public static bool operator !=({enumName} left, {enumName} right)");
						using (code.Block())
							code.WriteLine("return !left.Equals(right);");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Used for JSON serialization.");
						code.WriteLine($"public sealed class {enumName}JsonConverter : ServiceEnumJsonConverter<{enumName}>");
						using (code.Block())
						{
							code.WriteLine($"protected override {enumName} CreateCore(string value)");
							using (code.Block())
								code.WriteLine($"return new {enumName}(value);");
						}

						code.WriteLine();
						code.WriteLine("readonly string m_value;");
					}
				}

				return new ServiceTextSource(name: enumName + CSharpUtility.FileExtension, text: stringWriter.ToString());
			}
		}

		private ServiceTextSource GenerateDto(ServiceDtoInfo dtoInfo, Context context)
		{
			string fullDtoName = CSharpUtility.GetDtoName(dtoInfo);

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);

				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				var usings = new List<string>
				{
					"System",
					"System.Collections.Generic",
					"Facility.Core",
					"Newtonsoft.Json",
					"Newtonsoft.Json.Linq",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

				if (!dtoInfo.IsObsolete() && dtoInfo.Fields.Any(x => x.IsObsolete()))
				{
					CSharpUtility.WriteObsoletePragma(code);
					code.WriteLine();
				}

				code.WriteLine($"namespace {context.NamespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, dtoInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, dtoInfo);

					code.WriteLine($"public sealed partial class {fullDtoName} : ServiceDto<{fullDtoName}>");
					using (code.Block())
					{
						CSharpUtility.WriteSummary(code, "Creates an instance.");
						code.WriteLine($"public {fullDtoName}()");
						code.Block().Dispose();

						var fieldInfos = dtoInfo.Fields;
						GenerateFieldProperties(code, fieldInfos, context);

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Determines if two DTOs are equivalent.");
						code.WriteLine($"public override bool IsEquivalentTo({fullDtoName} other)");
						using (code.Block())
						{
							if (fieldInfos.Count == 0)
							{
								code.WriteLine("return other != null;");
							}
							else
							{
								code.WriteLine("return other != null &&");
								using (code.Indent())
								{
									for (int fieldIndex = 0; fieldIndex < fieldInfos.Count; fieldIndex++)
									{
										var fieldInfo = fieldInfos[fieldIndex];
										string propertyName = CSharpUtility.GetFieldPropertyName(fieldInfo);
										var fieldType = context.Service.GetFieldType(fieldInfo);
										if (fieldType.Kind == ServiceTypeKind.Array || fieldType.Kind == ServiceTypeKind.Map)
										{
											string outerAreEquivalentMethodName = fieldType.Kind == ServiceTypeKind.Map ? "AreEquivalentMaps" : "AreEquivalentArrays";
											string innerAreEquivalentMethodName = TryGetAreEquivalentMethodName(fieldType.ValueType.Kind);
											code.Write(innerAreEquivalentMethodName != null ?
												$"ServiceDataUtility.{outerAreEquivalentMethodName}({propertyName}, other.{propertyName}, ServiceDataUtility.{innerAreEquivalentMethodName})" :
												$"ServiceDataUtility.{outerAreEquivalentMethodName}({propertyName}, other.{propertyName})");
										}
										else
										{
											string areEquivalentMethodName = TryGetAreEquivalentMethodName(fieldType.Kind);
											code.Write(areEquivalentMethodName != null ?
												$"ServiceDataUtility.{areEquivalentMethodName}({propertyName}, other.{propertyName})" :
												$"{propertyName} == other.{propertyName}");
										}
										code.WriteLine(fieldIndex == fieldInfos.Count - 1 ? ";" : " &&");
									}
								}
							}
						}
					}
				}

				return new ServiceTextSource(name: fullDtoName + CSharpUtility.FileExtension, text: stringWriter.ToString());
			}
		}

		private ServiceTextSource GenerateHttpErrors(HttpErrorSetInfo httpErrorSetInfo, Context context)
		{
			var errorSetInfo = httpErrorSetInfo.ServiceErrorSet;

			string namespaceName = $"{CSharpUtility.GetNamespaceName(context.Service)}.{CSharpUtility.HttpDirectoryName}";
			string className = "Http" + errorSetInfo.Name;

			var namesAndStatusCodes = httpErrorSetInfo.Errors
				.Select(x => new { x.ServiceError.Name, x.StatusCode })
				.ToList();

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);

				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				var usings = new List<string>
				{
					"System",
					"System.Collections.Generic",
					"System.Net",
				};
				CSharpUtility.WriteUsings(code, usings, namespaceName);

				code.WriteLine($"namespace {namespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, errorSetInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, errorSetInfo);

					code.WriteLine($"public static partial class {className}");
					using (code.Block())
					{
						CSharpUtility.WriteSummary(code, "Gets the HTTP status code that corresponds to the specified error code.");
						code.WriteLine("public static HttpStatusCode? TryGetHttpStatusCode(string errorCode)");
						using (code.Block())
						{
							code.WriteLine("int statusCode;");
							code.WriteLine("return s_errorToStatus.TryGetValue(errorCode, out statusCode) ? (HttpStatusCode?) statusCode : null;");
						}

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Gets the error code that corresponds to the specified HTTP status code.");
						code.WriteLine("public static string TryGetErrorCode(HttpStatusCode statusCode)");
						using (code.Block())
						{
							code.WriteLine("switch ((int) statusCode)");
							using (code.Block())
							{
								foreach (var nameAndCodeGroup in namesAndStatusCodes.GroupBy(x => (int) x.StatusCode).OrderBy(x => x.Key))
								{
									string statusCode = nameAndCodeGroup.Key.ToString(CultureInfo.InvariantCulture);
									string errorCode = nameAndCodeGroup.Select(x => x.Name).First();
									code.WriteLine($"case {statusCode}: return \"{errorCode}\";");
								}

								code.WriteLine("default: return null;");
							}
						}

						code.WriteLine();
						code.WriteLine("static readonly IReadOnlyDictionary<string, int> s_errorToStatus = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)");
						using (code.Block("{", "};"))
						{
							foreach (var nameAndCode in namesAndStatusCodes)
							{
								string errorCode = nameAndCode.Name;
								string statusCode = ((int) nameAndCode.StatusCode).ToString(CultureInfo.InvariantCulture);
								code.WriteLine($"[\"{errorCode}\"] = {statusCode},");
							}
						}
					}
				}

				return new ServiceTextSource(name: $"{CSharpUtility.HttpDirectoryName}/{className}{CSharpUtility.FileExtension}", text: stringWriter.ToString());
			}
		}

		private ServiceTextSource GenerateHttpMapping(HttpServiceInfo httpServiceInfo, Context context)
		{
			var serviceInfo = httpServiceInfo.Service;

			string namespaceName = $"{CSharpUtility.GetNamespaceName(context.Service)}.{CSharpUtility.HttpDirectoryName}";
			string httpMappingName = $"{serviceInfo.Name}HttpMapping";

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);

				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				List<string> usings = new List<string>
				{
					"System",
					"System.Collections.Generic",
					"System.Globalization",
					"System.Net",
					"System.Net.Http",
					"Facility.Core",
					"Facility.Core.Http",
				};
				CSharpUtility.WriteUsings(code, usings, namespaceName);

				CSharpUtility.WriteObsoletePragma(code);
				code.WriteLine();

				code.WriteLine($"namespace {namespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, serviceInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, serviceInfo);

					code.WriteLine($"public static partial class {httpMappingName}");
					using (code.Block())
					{
						foreach (HttpMethodInfo httpMethodInfo in httpServiceInfo.Methods)
						{
							var methodInfo = httpMethodInfo.ServiceMethod;
							string methodName = CSharpUtility.GetMethodName(methodInfo);
							string requestTypeName = CSharpUtility.GetRequestDtoName(methodInfo);
							string responseTypeName = CSharpUtility.GetResponseDtoName(methodInfo);
							string httpPath = httpMethodInfo.Path;

							code.WriteLineSkipOnce();
							CSharpUtility.WriteSummary(code, methodInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, methodInfo);
							code.WriteLine($"public static readonly HttpMethodMapping<{requestTypeName}, {responseTypeName}> {methodName}Mapping =");
							using (code.Indent())
							{
								code.WriteLine($"new HttpMethodMapping<{requestTypeName}, {responseTypeName}>.Builder");
								code.WriteLine("{");
								using (code.Indent())
								{
									if (httpMethodInfo.Method == HttpMethod.Delete || httpMethodInfo.Method == HttpMethod.Get || httpMethodInfo.Method == HttpMethod.Head || httpMethodInfo.Method == HttpMethod.Options ||
										httpMethodInfo.Method == HttpMethod.Post || httpMethodInfo.Method == HttpMethod.Put || httpMethodInfo.Method == HttpMethod.Trace)
									{
										string httpMethodName = CodeGenUtility.Capitalize(httpMethodInfo.Method.ToString().ToLowerInvariant());
										code.WriteLine($"HttpMethod = HttpMethod.{httpMethodName},");
									}
									else
									{
										string httpMethodName = httpMethodInfo.Method.ToString();
										code.WriteLine($"HttpMethod = new HttpMethod(\"{httpMethodName}\"),");
									}

									code.WriteLine($"Path = \"{httpPath}\",");

									if (httpMethodInfo.PathFields.Count != 0)
									{
										code.WriteLine("ValidateRequest = request =>");
										code.WriteLine("{");
										using (code.Indent())
										{
											foreach (var pathField in httpMethodInfo.PathFields)
											{
												string fieldName = CSharpUtility.GetFieldPropertyName(pathField.ServiceField);
												code.WriteLine($"if (string.IsNullOrEmpty(request.{fieldName}))");
												using (code.Indent())
													code.WriteLine($"return ServiceResult.Failure(ServiceErrors.CreateRequestFieldRequired(\"{pathField.ServiceField.Name}\"));");
											}
											code.WriteLine("return ServiceResult.Success();");
										}
										code.WriteLine("},");
									}

									if (httpMethodInfo.PathFields.Count != 0 || httpMethodInfo.QueryFields.Count != 0)
									{
										code.WriteLine("GetUriParameters = request =>");
										using (code.Indent())
										{
											code.WriteLine("new Dictionary<string, string>");
											code.WriteLine("{");
											using (code.Indent())
											{
												foreach (var pathField in httpMethodInfo.PathFields)
												{
													string fieldName = CSharpUtility.GetFieldPropertyName(pathField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.Service.GetFieldType(pathField.ServiceField), $"request.{fieldName}");
													code.WriteLine($"{{ \"{pathField.ServiceField.Name}\", {fieldValue} }},");
												}
												foreach (var queryField in httpMethodInfo.QueryFields)
												{
													string fieldName = CSharpUtility.GetFieldPropertyName(queryField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.Service.GetFieldType(queryField.ServiceField), $"request.{fieldName}");
													code.WriteLine($"{{ \"{queryField.Name}\", {fieldValue} }},");
												}
											}
											code.WriteLine("},");
										}

										code.WriteLine("SetUriParameters = (request, parameters) =>");
										code.WriteLine("{");
										using (code.Indent())
										{
											foreach (var queryField in httpMethodInfo.QueryFields)
											{
												string dtoFieldName = CSharpUtility.GetFieldPropertyName(queryField.ServiceField);
												string queryParameterName = $"queryParameter{dtoFieldName}";
												code.WriteLine($"string {queryParameterName};");
												code.WriteLine($"parameters.TryGetValue(\"{queryField.Name}\", out {queryParameterName});");
												code.WriteLine($"request.{dtoFieldName} = {GenerateStringToFieldCode(context.Service.GetFieldType(queryField.ServiceField), queryParameterName)};");
											}

											foreach (var pathField in httpMethodInfo.PathFields)
											{
												string dtoFieldName = CSharpUtility.GetFieldPropertyName(pathField.ServiceField);
												string queryParameterName = $"queryParameter{dtoFieldName}";
												code.WriteLine($"string {queryParameterName};");
												code.WriteLine($"parameters.TryGetValue(\"{pathField.ServiceField.Name}\", out {queryParameterName});");
												code.WriteLine($"request.{dtoFieldName} = {GenerateStringToFieldCode(context.Service.GetFieldType(pathField.ServiceField), queryParameterName)};");
											}

											code.WriteLine("return request;");
										}
										code.WriteLine("},");
									}

									if (httpMethodInfo.RequestHeaderFields.Count != 0)
									{
										code.WriteLine("GetRequestHeaders = request =>");
										using (code.Indent())
										{
											code.WriteLine("new Dictionary<string, string>");
											code.WriteLine("{");
											using (code.Indent())
											{
												foreach (var headerField in httpMethodInfo.RequestHeaderFields)
												{
													string fieldName = CSharpUtility.GetFieldPropertyName(headerField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.Service.GetFieldType(headerField.ServiceField), $"request.{fieldName}");
													code.WriteLine($"{{ \"{headerField.Name}\", {fieldValue} }},");
												}
											}
											code.WriteLine("},");
										}

										code.WriteLine("SetRequestHeaders = (request, headers) =>");
										code.WriteLine("{");
										using (code.Indent())
										{
											foreach (var headerField in httpMethodInfo.RequestHeaderFields)
											{
												string dtoFieldName = CSharpUtility.GetFieldPropertyName(headerField.ServiceField);
												string headerVariableName = $"header{dtoFieldName}";
												code.WriteLine($"string {headerVariableName};");
												code.WriteLine($"headers.TryGetValue(\"{headerField.Name}\", out {headerVariableName});");
												code.WriteLine($"request.{dtoFieldName} = {GenerateStringToFieldCode(context.Service.GetFieldType(headerField.ServiceField), headerVariableName)};");
											}

											code.WriteLine("return request;");
										}
										code.WriteLine("},");
									}

									if (httpMethodInfo.RequestBodyField != null)
									{
										string requestBodyFieldName = CSharpUtility.GetFieldPropertyName(httpMethodInfo.RequestBodyField.ServiceField);
										string requestBodyFieldTypeName = CSharpUtility.GetDtoName(context.Service.GetFieldType(httpMethodInfo.RequestBodyField.ServiceField).Dto);

										code.WriteLine($"RequestBodyType = typeof({requestBodyFieldTypeName}),");
										code.WriteLine($"GetRequestBody = request => request.{requestBodyFieldName},");
										code.WriteLine($"CreateRequest = body => new {requestTypeName}{{ {requestBodyFieldName} = ({requestBodyFieldTypeName}) body }},");
									}
									else if (httpMethodInfo.RequestNormalFields.Any())
									{
										code.WriteLine($"RequestBodyType = typeof({requestTypeName}),");

										// copy fields if necessary; full request is the default
										if (httpMethodInfo.ServiceMethod.RequestFields.Count != httpMethodInfo.RequestNormalFields.Count)
										{
											code.WriteLine("GetRequestBody = request =>");
											using (code.Indent())
											{
												code.WriteLine($"new {requestTypeName}");
												code.WriteLine("{");
												using (code.Indent())
												{
													foreach (var field in httpMethodInfo.RequestNormalFields)
													{
														string fieldName = CSharpUtility.GetFieldPropertyName(field.ServiceField);
														code.WriteLine($"{fieldName} = request.{fieldName},");
													}
												}
												code.WriteLine($"}},");
											}

											code.WriteLine("CreateRequest = body =>");
											using (code.Indent())
											{
												code.WriteLine($"new {requestTypeName}");
												code.WriteLine("{");
												using (code.Indent())
												{
													foreach (var field in httpMethodInfo.RequestNormalFields)
													{
														string fieldName = CSharpUtility.GetFieldPropertyName(field.ServiceField);
														code.WriteLine($"{fieldName} = (({requestTypeName}) body).{fieldName},");
													}
												}
												code.WriteLine($"}},");
											}
										}
									}

									code.WriteLine("ResponseMappings =");
									code.WriteLine("{");
									using (code.Indent())
									{
										foreach (var validResponse in httpMethodInfo.ValidResponses)
										{
											code.WriteLine($"new HttpResponseMapping<{responseTypeName}>.Builder");
											code.WriteLine("{");

											using (code.Indent())
											{
												string statusCode = ((int) validResponse.StatusCode).ToString(CultureInfo.InvariantCulture);
												code.WriteLine($"StatusCode = (HttpStatusCode) {statusCode},");

												if (validResponse.ResponseBodyField != null)
												{
													string responseBodyFieldName = CSharpUtility.GetFieldPropertyName(validResponse.ResponseBodyField.ServiceField);

													if (context.Service.GetFieldType(validResponse.ResponseBodyField.ServiceField).Kind == ServiceTypeKind.Boolean)
													{
														code.WriteLine($"MatchesResponse = response => response.{responseBodyFieldName}.GetValueOrDefault(),");
														code.WriteLine($"CreateResponse = body => new {responseTypeName} {{ {responseBodyFieldName} = true }},");
													}
													else
													{
														string responseBodyFieldTypeName = CSharpUtility.GetDtoName(context.Service.GetFieldType(validResponse.ResponseBodyField.ServiceField).Dto);

														if (validResponse.HasResponseFields)
														{
															code.WriteLine($"ResponseBodyType = typeof({responseBodyFieldTypeName}),");
															code.WriteLine($"MatchesResponse = response => response.{responseBodyFieldName} != null,");
															code.WriteLine($"GetResponseBody = response => response.{responseBodyFieldName},");
															code.WriteLine($"CreateResponse = body => new {responseTypeName} {{ {responseBodyFieldName} = ({responseBodyFieldTypeName}) body }},");
														}
														else
														{
															code.WriteLine($"MatchesResponse = response => response.{responseBodyFieldName} != null,");
															code.WriteLine($"CreateResponse = body => new {responseTypeName} {{ {responseBodyFieldName} = new {responseBodyFieldTypeName}() }},");
														}
													}
												}
												else if (validResponse.HasResponseFields)
												{
													code.WriteLine($"ResponseBodyType = typeof({responseTypeName}),");

													// copy fields if necessary; full response is the default
													if (httpMethodInfo.ServiceMethod.ResponseFields.Count != httpMethodInfo.ResponseNormalFields.Count)
													{
														code.WriteLine("GetResponseBody = response =>");
														using (code.Indent())
														{
															code.WriteLine($"new {responseTypeName}");
															code.WriteLine("{");
															using (code.Indent())
															{
																foreach (var field in httpMethodInfo.ResponseNormalFields)
																{
																	string fieldName = CSharpUtility.GetFieldPropertyName(field.ServiceField);
																	code.WriteLine($"{fieldName} = response.{fieldName},");
																}
															}
															code.WriteLine($"}},");
														}

														code.WriteLine("CreateResponse = body =>");
														using (code.Indent())
														{
															code.WriteLine($"new {responseTypeName}");
															code.WriteLine("{");
															using (code.Indent())
															{
																foreach (var field in httpMethodInfo.ResponseNormalFields)
																{
																	string fieldName = CSharpUtility.GetFieldPropertyName(field.ServiceField);
																	code.WriteLine($"{fieldName} = (({responseTypeName}) body).{fieldName},");
																}
															}
															code.WriteLine($"}},");
														}
													}
												}
											}

											code.WriteLine("}.Build(),");
										}
									}
									code.WriteLine("},");

									if (httpMethodInfo.ResponseHeaderFields.Count != 0)
									{
										code.WriteLine("GetResponseHeaders = response =>");
										using (code.Indent())
										{
											code.WriteLine("new Dictionary<string, string>");
											code.WriteLine("{");
											using (code.Indent())
											{
												foreach (var headerField in httpMethodInfo.ResponseHeaderFields)
												{
													string fieldName = CSharpUtility.GetFieldPropertyName(headerField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.Service.GetFieldType(headerField.ServiceField), $"response.{fieldName}");
													code.WriteLine($"{{ \"{headerField.Name}\", {fieldValue} }},");
												}
											}
											code.WriteLine("},");
										}

										code.WriteLine("SetResponseHeaders = (response, headers) =>");
										code.WriteLine("{");
										using (code.Indent())
										{
											foreach (var headerField in httpMethodInfo.ResponseHeaderFields)
											{
												string dtoFieldName = CSharpUtility.GetFieldPropertyName(headerField.ServiceField);
												string headerVariableName = $"header{dtoFieldName}";
												code.WriteLine($"string {headerVariableName};");
												code.WriteLine($"headers.TryGetValue(\"{headerField.Name}\", out {headerVariableName});");
												code.WriteLine($"response.{dtoFieldName} = {GenerateStringToFieldCode(context.Service.GetFieldType(headerField.ServiceField), headerVariableName)};");
											}

											code.WriteLine("return response;");
										}
										code.WriteLine("},");
									}
								}
								code.WriteLine("}.Build();");
							}
						}
					}
				}

				return new ServiceTextSource(name: $"{CSharpUtility.HttpDirectoryName}/{httpMappingName}{CSharpUtility.FileExtension}", text: stringWriter.ToString());
			}
		}

		private string GenerateFieldToStringCode(ServiceTypeInfo serviceType, string fieldCode)
		{
			switch (serviceType.Kind)
			{
			case ServiceTypeKind.Enum:
			case ServiceTypeKind.Boolean:
				return $"{fieldCode} == null ? null : {fieldCode}.Value.ToString()";
			case ServiceTypeKind.Double:
			case ServiceTypeKind.Int32:
			case ServiceTypeKind.Int64:
				return $"{fieldCode} == null ? null : {fieldCode}.Value.ToString(CultureInfo.InvariantCulture)";
			case ServiceTypeKind.String:
				return fieldCode;
			default:
				throw new NotSupportedException("Unexpected field type: " + serviceType.Kind);
			}
		}

		private string GenerateStringToFieldCode(ServiceTypeInfo serviceType, string fieldCode)
		{
			switch (serviceType.Kind)
			{
			case ServiceTypeKind.Enum:
				string enumName = CSharpUtility.GetEnumName(serviceType.Enum);
				return $"{fieldCode} == null ? default({enumName}?) : new {enumName}({fieldCode})";
			case ServiceTypeKind.Boolean:
				return $"ServiceDataUtility.TryParseBoolean({fieldCode})";
			case ServiceTypeKind.Double:
				return $"ServiceDataUtility.TryParseDouble({fieldCode})";
			case ServiceTypeKind.Int32:
				return $"ServiceDataUtility.TryParseInt32({fieldCode})";
			case ServiceTypeKind.Int64:
				return $"ServiceDataUtility.TryParseInt64({fieldCode})";
			case ServiceTypeKind.String:
				return fieldCode;
			default:
				throw new NotSupportedException("Unexpected field type: " + serviceType.Kind);
			}
		}

		private ServiceTextSource GenerateHttpClient(HttpServiceInfo httpServiceInfo, Context context)
		{
			var serviceInfo = httpServiceInfo.Service;

			string namespaceName = $"{CSharpUtility.GetNamespaceName(context.Service)}.{CSharpUtility.HttpDirectoryName}";
			string fullServiceName = serviceInfo.Name;
			string fullHttpClientName = "HttpClient" + fullServiceName;
			string fullInterfaceName = CSharpUtility.GetInterfaceName(serviceInfo);
			string httpMappingName = serviceInfo.Name + "HttpMapping";

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);

				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				List<string> usings = new List<string>
				{
					"System",
					"System.Threading",
					"System.Threading.Tasks",
					"Facility.Core",
					"Facility.Core.Http",
				};
				CSharpUtility.WriteUsings(code, usings, namespaceName);

				code.WriteLine($"namespace {namespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, serviceInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, serviceInfo);

					code.WriteLine($"public sealed partial class {fullHttpClientName} : {fullInterfaceName}");
					using (code.Block())
					{
						string defaultUrl = httpServiceInfo.Url;

						if (defaultUrl != null)
						{
							CSharpUtility.WriteSummary(code, "Creates the service.");
							code.WriteLine($"public {fullHttpClientName}()");
							using (code.Indent())
								code.WriteLine(": this(null)");
							using (code.Block())
							{
							}
							code.WriteLine();
						}

						CSharpUtility.WriteSummary(code, "Creates the service.");
						code.WriteLine($"public {fullHttpClientName}(HttpClientServiceSettings settings)");
						using (code.Block())
						{
							if (defaultUrl != null)
								code.WriteLine($"m_httpClientService = new HttpClientService(settings, defaultBaseUri: new Uri(\"{defaultUrl}\"));");
							else
								code.WriteLine("m_httpClientService = new HttpClientService(settings);");
						}

						foreach (HttpMethodInfo httpMethodInfo in httpServiceInfo.Methods)
						{
							var methodInfo = httpMethodInfo.ServiceMethod;
							string methodName = CSharpUtility.GetMethodName(methodInfo);
							string requestTypeName = CSharpUtility.GetRequestDtoName(methodInfo);
							string responseTypeName = CSharpUtility.GetResponseDtoName(methodInfo);

							code.WriteLine();
							CSharpUtility.WriteSummary(code, methodInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, methodInfo);
							code.WriteLine($"public Task<ServiceResult<{responseTypeName}>> {methodName}Async({requestTypeName} request, CancellationToken cancellationToken)");
							using (code.Block())
								code.WriteLine($"return m_httpClientService.TrySendRequestAsync({httpMappingName}.{methodName}Mapping, request, cancellationToken);");
						}

						code.WriteLine();
						code.WriteLine("readonly HttpClientService m_httpClientService;");
					}
				}

				return new ServiceTextSource(name: $"{CSharpUtility.HttpDirectoryName}/{fullHttpClientName}{CSharpUtility.FileExtension}", text: stringWriter.ToString());
			}
		}

		private ServiceTextSource GenerateHttpHandler(HttpServiceInfo httpServiceInfo, Context context)
		{
			var serviceInfo = httpServiceInfo.Service;

			string namespaceName = $"{CSharpUtility.GetNamespaceName(context.Service)}.{CSharpUtility.HttpDirectoryName}";
			string fullServiceName = serviceInfo.Name;
			string fullHttpHandlerName = fullServiceName + "HttpHandler";
			string fullInterfaceName = CSharpUtility.GetInterfaceName(serviceInfo);
			string httpMappingName = serviceInfo.Name + "HttpMapping";

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);

				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				List<string> usings = new List<string>
				{
					"System",
					"System.Net",
					"System.Net.Http",
					"System.Threading",
					"System.Threading.Tasks",
					"Facility.Core.Http",
				};
				CSharpUtility.WriteUsings(code, usings, namespaceName);

				if (serviceInfo.Methods.Any(x => x.IsObsolete()))
				{
					CSharpUtility.WriteObsoletePragma(code);
					code.WriteLine();
				}

				code.WriteLine($"namespace {namespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, serviceInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, serviceInfo);

					code.WriteLine($"public sealed partial class {fullHttpHandlerName} : ServiceHttpHandler");
					using (code.Block())
					{
						CSharpUtility.WriteSummary(code, "Creates the handler.");
						code.WriteLine($"public {fullHttpHandlerName}({fullInterfaceName} service, ServiceHttpHandlerSettings settings)");
						using (code.Indent())
							code.WriteLine(": base(settings)");
						using (code.Block())
						{
							code.WriteLine("if (service == null)");
							using (code.Indent())
								code.WriteLine("throw new ArgumentNullException(\"service\");");

							code.WriteLine();
							code.WriteLine("m_service = service;");
						}

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Attempts to handle the HTTP request.");
						code.WriteLine("public override async Task<HttpResponseMessage> TryHandleHttpRequestAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)");
						using (code.Block())
						{
							// check 'widgets/get' before 'widgets/{id}'
							IDisposable indent = null;
							code.Write("return ");
							foreach (var httpServiceMethod in httpServiceInfo.Methods.OrderBy(x => x, HttpMethodInfo.ByRouteComparer))
							{
								if (indent != null)
									code.WriteLine(" ??");
								string methodName = CSharpUtility.GetMethodName(httpServiceMethod.ServiceMethod);
								code.Write($"await AdaptTask(TryHandle{methodName}Async(httpRequest, cancellationToken)).ConfigureAwait(true)");
								if (indent == null)
									indent = code.Indent();
							}
							code.WriteLine(";");
							indent?.Dispose();
						}

						foreach (HttpMethodInfo httpMethodInfo in httpServiceInfo.Methods)
						{
							var methodInfo = httpMethodInfo.ServiceMethod;
							string methodName = CSharpUtility.GetMethodName(methodInfo);

							code.WriteLine();
							CSharpUtility.WriteSummary(code, methodInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, methodInfo);
							code.WriteLine($"public Task<HttpResponseMessage> TryHandle{methodName}Async(HttpRequestMessage httpRequest, CancellationToken cancellationToken)");
							using (code.Block())
								code.WriteLine($"return TryHandleServiceMethodAsync({httpMappingName}.{methodName}Mapping, httpRequest, m_service.{methodName}Async, cancellationToken);");
						}

						if (httpServiceInfo.ErrorSets.Count != 0)
						{
							code.WriteLine();
							CSharpUtility.WriteSummary(code, "Returns the HTTP status code for a custom error code.");
							code.WriteLine("protected override HttpStatusCode? TryGetCustomHttpStatusCode(string errorCode)");
							using (code.Block())
							{
								string tryGetCustomHttpStatusCode = string.Join(" ?? ", httpServiceInfo.ErrorSets.Select(x => $"Http{x.ServiceErrorSet.Name}.TryGetHttpStatusCode(errorCode)"));
								code.WriteLine($"return {tryGetCustomHttpStatusCode};");
							}
						}

						code.WriteLine();
						code.WriteLine($"readonly {fullInterfaceName} m_service;");
					}
				}

				return new ServiceTextSource(name: $"{CSharpUtility.HttpDirectoryName}/{fullHttpHandlerName}{CSharpUtility.FileExtension}", text: stringWriter.ToString());
			}
		}

		private static string TryGetAreEquivalentMethodName(ServiceTypeKind kind)
		{
			switch (kind)
			{
			case ServiceTypeKind.Bytes:
				return "AreEquivalentBytes";
			case ServiceTypeKind.Object:
				return "AreEquivalentObjects";
			case ServiceTypeKind.Error:
			case ServiceTypeKind.Dto:
				return "AreEquivalentDtos";
			case ServiceTypeKind.Result:
				return "AreEquivalentResults";
			case ServiceTypeKind.Array:
			case ServiceTypeKind.Map:
				throw new InvalidOperationException("Collections of collections not supported.");
			default:
				return null;
			}
		}

		private IEnumerable<ServiceTextSource> GenerateMethodDtos(ServiceMethodInfo methodInfo, Context context)
		{
			yield return GenerateDto(new ServiceDtoInfo(
				name: $"{CodeGenUtility.Capitalize(methodInfo.Name)}Request",
				fields: methodInfo.RequestFields,
				summary: $"Request for {CodeGenUtility.Capitalize(methodInfo.Name)}.",
				position: methodInfo.Position), context);

			yield return GenerateDto(new ServiceDtoInfo(
				name: $"{CodeGenUtility.Capitalize(methodInfo.Name)}Response",
				fields: methodInfo.ResponseFields,
				summary: $"Response for {CodeGenUtility.Capitalize(methodInfo.Name)}.",
				position: methodInfo.Position), context);
		}

		private void GenerateFieldProperties(CodeWriter code, IEnumerable<ServiceFieldInfo> fieldInfos, Context context)
		{
			foreach (var fieldInfo in fieldInfos)
			{
				string propertyName = CSharpUtility.GetFieldPropertyName(fieldInfo);
				string normalPropertyName = CodeGenUtility.Capitalize(fieldInfo.Name);
				string nullableFieldType = RenderNullableFieldType(context.Service.GetFieldType(fieldInfo));

				code.WriteLine();
				CSharpUtility.WriteSummary(code, fieldInfo.Summary);
				if (fieldInfo.IsObsolete())
					code.WriteLine("[Obsolete]");
				if (propertyName != normalPropertyName)
					code.WriteLine($"[JsonProperty(\"{fieldInfo.Name}\")]");
				code.WriteLine($"public {nullableFieldType} {propertyName} {{ get; set; }}");
			}
		}

		private ServiceTextSource GenerateInterface(ServiceInfo serviceInfo, Context context)
		{
			string interfaceName = CSharpUtility.GetInterfaceName(serviceInfo);

			using (var stringWriter = new StringWriter())
			{
				var code = new CodeWriter(stringWriter);
				CSharpUtility.WriteFileHeader(code, context.GeneratorName);

				var usings = new List<string>
				{
					"System",
					"System.Threading",
					"System.Threading.Tasks",
					"Facility.Core",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

				code.WriteLine($"namespace {context.NamespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteSummary(code, serviceInfo.Summary);
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, serviceInfo);

					code.WriteLine($"public partial interface {interfaceName}");
					using (code.Block())
					{
						foreach (ServiceMethodInfo methodInfo in serviceInfo.Methods)
						{
							code.WriteLineSkipOnce();
							CSharpUtility.WriteSummary(code, methodInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, methodInfo);
							code.WriteLine($"Task<ServiceResult<{CSharpUtility.GetResponseDtoName(methodInfo)}>> {CSharpUtility.GetMethodName(methodInfo)}Async(" +
								$"{CSharpUtility.GetRequestDtoName(methodInfo)} request, CancellationToken cancellationToken);");
						}
					}
				}

				return new ServiceTextSource(name: interfaceName + CSharpUtility.FileExtension, text: stringWriter.ToString());
			}
		}

		private string RenderNonNullableFieldType(ServiceTypeInfo fieldType)
		{
			return RenderNullableFieldType(fieldType).TrimEnd('?');
		}

		private string RenderNullableFieldType(ServiceTypeInfo fieldType)
		{
			switch (fieldType.Kind)
			{
			case ServiceTypeKind.String:
				return "string";
			case ServiceTypeKind.Boolean:
				return "bool?";
			case ServiceTypeKind.Double:
				return "double?";
			case ServiceTypeKind.Int32:
				return "int?";
			case ServiceTypeKind.Int64:
				return "long?";
			case ServiceTypeKind.Bytes:
				return "byte[]";
			case ServiceTypeKind.Object:
				return "JObject";
			case ServiceTypeKind.Error:
				return "ServiceErrorDto";
			case ServiceTypeKind.Dto:
				return CSharpUtility.GetDtoName(fieldType.Dto);
			case ServiceTypeKind.Enum:
				return CSharpUtility.GetEnumName(fieldType.Enum) + "?";
			case ServiceTypeKind.Result:
				return $"ServiceResult<{RenderNonNullableFieldType(fieldType.ValueType)}>";
			case ServiceTypeKind.Array:
				return $"IReadOnlyList<{RenderNonNullableFieldType(fieldType.ValueType)}>";
			case ServiceTypeKind.Map:
				return $"IReadOnlyDictionary<string, {RenderNonNullableFieldType(fieldType.ValueType)}>";
			default:
				throw new NotSupportedException("Unknown field type " + fieldType.Kind);
			}
		}

		private sealed class Context
		{
			public string GeneratorName { get; set; }

			public string NamespaceName { get; set; }

			public ServiceInfo Service { get; set; }
		}
	}
}
