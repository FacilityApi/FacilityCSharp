using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Http;

namespace Facility.CodeGen.CSharp
{
	/// <summary>
	/// Generates C#.
	/// </summary>
	public sealed class CSharpGenerator : CodeGenerator
	{
		/// <summary>
		/// Generates C#.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <returns>The number of updated files.</returns>
		public static int GenerateCSharp(CSharpGeneratorSettings settings) =>
			FileGenerator.GenerateFiles(new CSharpGenerator { GeneratorName = nameof(CSharpGenerator) }, settings);

		/// <summary>
		/// The name of the namespace (optional).
		/// </summary>
		public string? NamespaceName { get; set; }

		/// <summary>
		/// True if the code should use nullable reference syntax.
		/// </summary>
		public bool UseNullableReferences { get; set; }

		/// <summary>
		/// Generates the C# output.
		/// </summary>
		public override CodeGenOutput GenerateOutput(ServiceInfo service)
		{
			var outputFiles = new List<CodeGenFile>();

			var context = new Context(GeneratorName ?? "", CSharpServiceInfo.Create(service), NamespaceName);

			foreach (var errorSetInfo in service.ErrorSets.Where(x => x.Errors.Count != 0))
				outputFiles.Add(GenerateErrorSet(errorSetInfo, context));

			foreach (var enumInfo in service.Enums.Where(x => x.Values.Count != 0))
				outputFiles.Add(GenerateEnum(enumInfo, context));

			foreach (var dtoInfo in service.Dtos)
				outputFiles.Add(GenerateDto(dtoInfo, context));

			if (service.Methods.Count != 0)
			{
				outputFiles.Add(GenerateInterface(service, context));

				foreach (var methodInfo in service.Methods)
					outputFiles.AddRange(GenerateMethodDtos(methodInfo, context));

				outputFiles.Add(GenerateMethodInfos(service, context));
				outputFiles.Add(GenerateDelegatingService(service, context));
			}

			var httpServiceInfo = HttpServiceInfo.Create(service);

			foreach (var httpErrorSetInfo in httpServiceInfo.ErrorSets)
				outputFiles.Add(GenerateHttpErrors(httpErrorSetInfo, context));

			if (httpServiceInfo.Methods.Count != 0)
			{
				outputFiles.Add(GenerateHttpMapping(httpServiceInfo, context));
				outputFiles.Add(GenerateHttpClient(httpServiceInfo, context));
				outputFiles.Add(GenerateHttpHandler(httpServiceInfo, context));
			}

			string codeGenComment = CodeGenUtility.GetCodeGenComment(context.GeneratorName);
			var patternsToClean = new[]
			{
				new CodeGenPattern("*.g.cs", codeGenComment),
				new CodeGenPattern("Http/*.g.cs", codeGenComment),
			};
			return new CodeGenOutput(outputFiles, patternsToClean);
		}

		/// <summary>
		/// Applies generator-specific settings.
		/// </summary>
		public override void ApplySettings(FileGeneratorSettings settings)
		{
			var csharpSettings = (CSharpGeneratorSettings) settings;
			NamespaceName = csharpSettings.NamespaceName;
			UseNullableReferences = csharpSettings.UseNullableReferences;
		}

		/// <summary>
		/// Patterns to clean are returned with the output.
		/// </summary>
		public override bool HasPatternsToClean => true;

		private CodeGenFile GenerateErrorSet(ServiceErrorSetInfo errorSetInfo, Context context)
		{
			string fullErrorSetName = CSharpUtility.GetErrorSetName(errorSetInfo);

			return CreateFile(fullErrorSetName + CSharpUtility.FileExtension, code =>
			{
				WriteFileHeader(code, context);

				var usings = new List<string>
				{
					"System",
					"Facility.Core",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

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
							code.WriteLine($"public static ServiceErrorDto Create{memberName}(string{NullableReferenceSuffix} message = null) => " +
								$"new ServiceErrorDto({memberName}, message" +
								$"{(string.IsNullOrWhiteSpace(errorInfo.Summary) ? "" : $" ?? {CSharpUtility.CreateString(errorInfo.Summary)}")});");
						}
					}
				}
			});
		}

		private CodeGenFile GenerateEnum(ServiceEnumInfo enumInfo, Context context)
		{
			string enumName = CSharpUtility.GetEnumName(enumInfo);

			return CreateFile(enumName + CSharpUtility.FileExtension, code =>
			{
				WriteFileHeader(code, context);

				var usings = new List<string>
				{
					"System",
					"System.Collections.Generic",
					"System.Collections.ObjectModel",
					"Facility.Core",
					"Newtonsoft.Json",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

				if (!enumInfo.IsObsolete && enumInfo.Values.Any(x => x.IsObsolete))
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
					code.WriteLine($"public partial struct {enumName} : IEquatable<{enumName}>");
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
						code.WriteLine($"public {enumName}(string value) => m_value = value;");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Converts the instance to a string.");
						code.WriteLine("public override string ToString() => m_value ?? \"\";");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for equality.");
						code.WriteLine($"public bool Equals({enumName} other) => StringComparer.OrdinalIgnoreCase.Equals(ToString(), other.ToString());");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for equality.");
						code.WriteLine($"public override bool Equals(object obj) => obj is {enumName} && Equals(({enumName}) obj);");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Gets the hash code.");
						code.WriteLine("public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for equality.");
						code.WriteLine($"public static bool operator ==({enumName} left, {enumName} right) => left.Equals(right);");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Checks for inequality.");
						code.WriteLine($"public static bool operator !=({enumName} left, {enumName} right) => !left.Equals(right);");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Returns true if the instance is equal to one of the defined values.");
						code.WriteLine("public bool IsDefined() => s_values.Contains(this);");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Returns all of the defined values.");
						code.WriteLine($"public static IReadOnlyList<{enumName}> GetValues() => s_values;");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Used for JSON serialization.");
						code.WriteLine($"public sealed class {enumName}JsonConverter : ServiceEnumJsonConverter<{enumName}>");
						using (code.Block())
						{
							CSharpUtility.WriteSummary(code, "Creates the value from a string.");
							code.WriteLine($"protected override {enumName} CreateCore(string value) => new {enumName}(value);");
						}

						code.WriteLine();
						code.WriteLine($"private static readonly ReadOnlyCollection<{enumName}> s_values = new ReadOnlyCollection<{enumName}>(");
						using (code.Indent())
						{
							code.WriteLine("new[]");
							using (code.Block("{", "});"))
							{
								foreach (var value in enumInfo.Values)
									code.WriteLine($"{CSharpUtility.GetEnumValueName(value)},");
							}
						}

						code.WriteLine();
						code.WriteLine("readonly string m_value;");
					}
				}
			});
		}

		private CodeGenFile GenerateDto(ServiceDtoInfo dtoInfo, Context context)
		{
			string fullDtoName = CSharpUtility.GetDtoName(dtoInfo);

			return CreateFile(fullDtoName + CSharpUtility.FileExtension, code =>
			{
				WriteFileHeader(code, context);

				var usings = new List<string>
				{
					"System",
					"System.Collections.Generic",
					"Facility.Core",
					"Newtonsoft.Json",
					"Newtonsoft.Json.Linq",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

				if (!dtoInfo.IsObsolete && dtoInfo.Fields.Any(x => x.IsObsolete))
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
						code.WriteLine($"public override bool IsEquivalentTo({NullableReference(fullDtoName)} other)");
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
									for (var fieldIndex = 0; fieldIndex < fieldInfos.Count; fieldIndex++)
									{
										var fieldInfo = fieldInfos[fieldIndex];
										var propertyName = context.GetFieldPropertyName(fieldInfo);
										var fieldType = context.GetFieldType(fieldInfo);
										var areEquivalentMethodName = TryGetAreEquivalentMethodName(fieldType!.Kind);
										code.Write(areEquivalentMethodName != null ?
											$"ServiceDataUtility.{areEquivalentMethodName}({propertyName}, other.{propertyName})" :
											$"{propertyName} == other.{propertyName}");
										code.WriteLine(fieldIndex == fieldInfos.Count - 1 ? ";" : " &&");
									}
								}
							}
						}

						var requiredFields = fieldInfos.Where(x => x.IsRequired).ToList();
						var validateFields = fieldInfos.Where(x => x.Validation != null).ToList();
						var fieldsRequiringRecursiveValidation = fieldInfos.Where(x => context.NeedsValidation(context.GetFieldType(x))).ToList();

						if (requiredFields.Count != 0 || validateFields.Count != 0 || fieldsRequiringRecursiveValidation.Count != 0)
						{
							code.WriteLine();
							CSharpUtility.WriteSummary(code, "Validates the DTO.");
							code.WriteLine($"public override bool Validate(out string{NullableReferenceSuffix} errorMessage)");
							using (code.Block())
							{
								code.WriteLine("errorMessage = GetValidationErrorMessage();");
								code.WriteLine("return errorMessage == null;");
							}

							code.WriteLine();
							code.WriteLine($"private string{NullableReferenceSuffix} GetValidationErrorMessage()");
							using (code.Block())
							{
								if (requiredFields.Count != 0)
								{
									code.WriteLineSkipOnce();
									foreach (var fieldInfo in requiredFields)
									{
										var propertyName = context.GetFieldPropertyName(fieldInfo);
										code.WriteLine($"if ({propertyName} == null)");
										using (code.Indent())
											code.WriteLine($"return ServiceDataUtility.GetRequiredFieldErrorMessage(\"{fieldInfo.Name}\");");
									}
								}

								if (validateFields.Count != 0)
								{
									code.WriteLineSkipOnce();
									foreach (var fieldInfo in validateFields)
									{
										var type = context.GetFieldType(fieldInfo);
										var propertyName = context.GetFieldPropertyName(fieldInfo);

										switch (type.Kind)
										{
											case ServiceTypeKind.Enum:
											{
												code.WriteLine($"if (!{propertyName}.IsDefined())");
												using (code.Indent())
													code.WriteLine($"return ServiceDataUtility.GetInvalidFieldErrorMessage(\"{fieldInfo.Name}\");");

												break;
											}

											case ServiceTypeKind.String:
											{
												var validRange = fieldInfo.Validation!.LengthRange;
												if (validRange != null)
												{
													code.WriteLine($"if ({propertyName}.Length is < {validRange.Minimum} or > {validRange.Maximum})");
													using (code.Indent())
														code.WriteLine($"return ServiceDataUtility.GetInvalidFieldErrorMessage(\"{fieldInfo.Name}\");");
												}

												var validPattern = fieldInfo.Validation.RegexPattern;
												if (validPattern != null)
												{
													code.WriteLine($"if (!System.Text.RegularExpressions.Regex.IsMatch({propertyName}, \"{validPattern}\")");
													using (code.Indent())
														code.WriteLine($"return ServiceDataUtility.GetInvalidFieldErrorMessage(\"{fieldInfo.Name}\");");
												}

												break;
											}

											case ServiceTypeKind.Double:
											case ServiceTypeKind.Int32:
											case ServiceTypeKind.Int64:
											case ServiceTypeKind.Decimal:
											{
												var validRange = fieldInfo.Validation!.ValueRange!;
												code.WriteLine($"if ({propertyName} is < {validRange.Minimum} or > {validRange.Maximum})");
												using (code.Indent())
													code.WriteLine($"return ServiceDataUtility.GetInvalidFieldErrorMessage(\"{fieldInfo.Name}\");");

												break;
											}

											case ServiceTypeKind.Bytes:
											case ServiceTypeKind.Array:
											case ServiceTypeKind.Map:
											{
												var validRange = fieldInfo.Validation!.CountRange!;
												code.WriteLine($"if ({propertyName}.Count is < {validRange.Minimum} or > {validRange.Maximum})");
												using (code.Indent())
													code.WriteLine($"return ServiceDataUtility.GetInvalidFieldErrorMessage(\"{fieldInfo.Name}\");");

												break;
											}
										}
									}
								}

								if (fieldsRequiringRecursiveValidation.Count != 0)
								{
									code.WriteLineSkipOnce();
									code.WriteLine($"string{NullableReferenceSuffix} errorMessage;");
									foreach (var fieldInfo in fieldsRequiringRecursiveValidation)
									{
										var propertyName = context.GetFieldPropertyName(fieldInfo);
										code.WriteLine($"if (!ServiceDataUtility.ValidateFieldValue({propertyName}, \"{fieldInfo.Name}\", out errorMessage))");
										using (code.Indent())
											code.WriteLine($"return errorMessage{NullableReferenceBang};");
									}
								}

								code.WriteLine();
								code.WriteLine("return null;");
							}
						}
					}
				}
			});
		}

		private CodeGenFile GenerateHttpErrors(HttpErrorSetInfo httpErrorSetInfo, Context context)
		{
			var errorSetInfo = httpErrorSetInfo.ServiceErrorSet;

			string namespaceName = $"{context.NamespaceName}.{CSharpUtility.HttpDirectoryName}";
			string className = "Http" + errorSetInfo.Name;

			var errorsAndStatusCodes = httpErrorSetInfo.Errors
				.Select(x => new { x.ServiceError, x.StatusCode })
				.ToList();

			return CreateFile($"{CSharpUtility.HttpDirectoryName}/{className}{CSharpUtility.FileExtension}", code =>
			{
				WriteFileHeader(code, context);

				var usings = new List<string>
				{
					"System",
					"System.Collections.Generic",
					"System.Net",
				};
				CSharpUtility.WriteUsings(code, usings, namespaceName);

				if (!errorSetInfo.IsObsolete && errorSetInfo.Errors.Any(x => x.IsObsolete))
				{
					CSharpUtility.WriteObsoletePragma(code);
					code.WriteLine();
				}

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
						code.WriteLine($"public static HttpStatusCode? TryGetHttpStatusCode(string{NullableReferenceSuffix} errorCode) =>");
						using (code.Indent())
							code.WriteLine("s_errorToStatus.TryGetValue(errorCode ?? \"\", out var statusCode) ? (HttpStatusCode?) statusCode : null;");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Gets the error code that corresponds to the specified HTTP status code.");
						code.WriteLine($"public static string{NullableReferenceSuffix} TryGetErrorCode(HttpStatusCode statusCode)");
						using (code.Block())
						{
							code.WriteLine("switch ((int) statusCode)");
							code.WriteLine("{");

							foreach (var errorAndCodeGroup in errorsAndStatusCodes.GroupBy(x => (int) x.StatusCode).OrderBy(x => x.Key))
							{
								string statusCode = errorAndCodeGroup.Key.ToString(CultureInfo.InvariantCulture);
								code.WriteLine($"case {statusCode}:");
								using (code.Indent())
									code.WriteLine($"return {CSharpUtility.GetErrorSetName(errorSetInfo)}.{CSharpUtility.GetErrorName(errorAndCodeGroup.First().ServiceError)};");
							}

							code.WriteLine("default:");
							using (code.Indent())
								code.WriteLine("return null;");

							code.WriteLine("}");
						}

						code.WriteLine();
						code.WriteLine("private static readonly IReadOnlyDictionary<string, int> s_errorToStatus = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)");
						using (code.Block("{", "};"))
						{
							foreach (var errorAndStatusCode in errorsAndStatusCodes)
							{
								string statusCode = ((int) errorAndStatusCode.StatusCode).ToString(CultureInfo.InvariantCulture);
								code.WriteLine($"{{ {CSharpUtility.GetErrorSetName(errorSetInfo)}.{CSharpUtility.GetErrorName(errorAndStatusCode.ServiceError)}, {statusCode} }},");
							}
						}
					}
				}
			});
		}

		private CodeGenFile GenerateHttpMapping(HttpServiceInfo httpServiceInfo, Context context)
		{
			var serviceInfo = httpServiceInfo.Service;

			string namespaceName = $"{context.NamespaceName}.{CSharpUtility.HttpDirectoryName}";
			string httpMappingName = $"{serviceInfo.Name}HttpMapping";

			return CreateFile($"{CSharpUtility.HttpDirectoryName}/{httpMappingName}{CSharpUtility.FileExtension}", code =>
			{
				WriteFileHeader(code, context);

				List<string> usings = new List<string>
				{
					"System",
					"System.Collections.Generic",
					"System.Globalization",
					"System.Net",
					"System.Net.Http",
					"Facility.Core",
					"Facility.Core.Http",
					"Newtonsoft.Json.Linq",
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
								using (code.Block("{", "}.Build();"))
								{
									if (httpMethodInfo.Method == HttpMethod.Delete.ToString() || httpMethodInfo.Method == HttpMethod.Get.ToString() ||
										httpMethodInfo.Method == HttpMethod.Post.ToString() || httpMethodInfo.Method == HttpMethod.Put.ToString())
									{
										string httpMethodName = CodeGenUtility.Capitalize(httpMethodInfo.Method.ToLowerInvariant());
										code.WriteLine($"HttpMethod = HttpMethod.{httpMethodName},");
									}
									else
									{
										string httpMethodName = httpMethodInfo.Method;
										code.WriteLine($"HttpMethod = new HttpMethod(\"{httpMethodName}\"),");
									}

									code.WriteLine($"Path = \"{httpPath}\",");

									if (httpMethodInfo.PathFields.Count != 0 || httpMethodInfo.RequestBodyField != null)
									{
										code.WriteLine("ValidateRequest = request =>");
										using (code.Block("{", "},"))
										{
											foreach (var pathField in httpMethodInfo.PathFields)
											{
												var serviceField = pathField.ServiceField;
												string fieldName = context.GetFieldPropertyName(serviceField);
												var fieldType = context.GetFieldType(serviceField);
												if (fieldType.Kind == ServiceTypeKind.String)
													code.WriteLine($"if (string.IsNullOrEmpty(request.{fieldName}))");
												else
													code.WriteLine($"if (request.{fieldName} == null)");
												using (code.Indent())
													code.WriteLine($"return ServiceResult.Failure(ServiceErrors.CreateRequestFieldRequired(\"{serviceField.Name}\"));");
											}

											if (httpMethodInfo.RequestBodyField != null)
											{
												var serviceField = httpMethodInfo.RequestBodyField.ServiceField;
												string fieldName = context.GetFieldPropertyName(serviceField);
												code.WriteLine($"if (request.{fieldName} == null)");
												using (code.Indent())
													code.WriteLine($"return ServiceResult.Failure(ServiceErrors.CreateRequestFieldRequired(\"{serviceField.Name}\"));");
											}

											code.WriteLine("return ServiceResult.Success();");
										}
									}

									if (httpMethodInfo.PathFields.Count != 0 || httpMethodInfo.QueryFields.Count != 0)
									{
										code.WriteLine("GetUriParameters = request =>");
										using (code.Indent())
										{
											code.WriteLine($"new Dictionary<string, string{NullableReferenceSuffix}>");
											using (code.Block("{", "},"))
											{
												foreach (var pathField in httpMethodInfo.PathFields)
												{
													string fieldName = context.GetFieldPropertyName(pathField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.GetFieldType(pathField.ServiceField), $"request.{fieldName}");
													code.WriteLine($"{{ \"{pathField.Name}\", {fieldValue} }},");
												}
												foreach (var queryField in httpMethodInfo.QueryFields)
												{
													string fieldName = context.GetFieldPropertyName(queryField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.GetFieldType(queryField.ServiceField), $"request.{fieldName}");
													code.WriteLine($"{{ \"{queryField.Name}\", {fieldValue} }},");
												}
											}
										}

										code.WriteLine("SetUriParameters = (request, parameters) =>");
										using (code.Block("{", "},"))
										{
											foreach (var queryField in httpMethodInfo.QueryFields)
											{
												string dtoFieldName = context.GetFieldPropertyName(queryField.ServiceField);
												string queryParameterName = $"queryParameter{dtoFieldName}";
												code.WriteLine($"parameters.TryGetValue(\"{queryField.Name}\", out var {queryParameterName});");
												code.WriteLine($"request.{dtoFieldName} = {GenerateStringToFieldCode(context.GetFieldType(queryField.ServiceField), queryParameterName)};");
											}

											foreach (var pathField in httpMethodInfo.PathFields)
											{
												string dtoFieldName = context.GetFieldPropertyName(pathField.ServiceField);
												string queryParameterName = $"queryParameter{dtoFieldName}";
												code.WriteLine($"parameters.TryGetValue(\"{pathField.Name}\", out var {queryParameterName});");
												code.WriteLine($"request.{dtoFieldName} = {GenerateStringToFieldCode(context.GetFieldType(pathField.ServiceField), queryParameterName)};");
											}

											code.WriteLine("return request;");
										}
									}

									if (httpMethodInfo.RequestHeaderFields.Count != 0)
									{
										code.WriteLine("GetRequestHeaders = request =>");
										using (code.Indent())
										{
											code.WriteLine($"new Dictionary<string, string{NullableReferenceSuffix}>");
											using (code.Block("{", "},"))
											{
												foreach (var headerField in httpMethodInfo.RequestHeaderFields)
												{
													string fieldName = context.GetFieldPropertyName(headerField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.GetFieldType(headerField.ServiceField), $"request.{fieldName}");
													code.WriteLine($"[\"{headerField.Name}\"] = {fieldValue},");
												}
											}
										}

										code.WriteLine("SetRequestHeaders = (request, headers) =>");
										using (code.Block("{", "},"))
										{
											foreach (var headerField in httpMethodInfo.RequestHeaderFields)
											{
												string dtoFieldName = context.GetFieldPropertyName(headerField.ServiceField);
												string headerVariableName = $"header{dtoFieldName}";
												code.WriteLine($"headers.TryGetValue(\"{headerField.Name}\", out var {headerVariableName});");
												code.WriteLine($"request.{dtoFieldName} = {GenerateStringToFieldCode(context.GetFieldType(headerField.ServiceField), headerVariableName)};");
											}

											code.WriteLine("return request;");
										}
									}

									if (httpMethodInfo.RequestBodyField != null)
									{
										var requestBodyFieldName = context.GetFieldPropertyName(httpMethodInfo.RequestBodyField.ServiceField);
										var requestBodyFieldInfo = context.GetFieldType(httpMethodInfo.RequestBodyField.ServiceField);
										var requestBodyFieldTypeName = RenderNullableFieldType(requestBodyFieldInfo);
										var requestNullableBodyFieldTypeName = RenderNullableReferenceFieldType(requestBodyFieldInfo);

										code.WriteLine($"RequestBodyType = typeof({requestBodyFieldTypeName}),");
										if (httpMethodInfo.RequestBodyField.ContentType != null)
											code.WriteLine($"RequestBodyContentType = {CSharpUtility.CreateString(httpMethodInfo.RequestBodyField.ContentType)},");
										code.WriteLine($"GetRequestBody = request => request.{requestBodyFieldName},");
										code.WriteLine($"CreateRequest = body => new {requestTypeName} {{ {requestBodyFieldName} = ({requestNullableBodyFieldTypeName}) body }},");
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
												using (code.Block("{", "},"))
												{
													foreach (var field in httpMethodInfo.RequestNormalFields)
													{
														string fieldName = context.GetFieldPropertyName(field.ServiceField);
														code.WriteLine($"{fieldName} = request.{fieldName},");
													}
												}
											}

											code.WriteLine("CreateRequest = body =>");
											using (code.Indent())
											{
												code.WriteLine($"new {requestTypeName}");
												using (code.Block("{", "},"))
												{
													foreach (var field in httpMethodInfo.RequestNormalFields)
													{
														string fieldName = context.GetFieldPropertyName(field.ServiceField);
														code.WriteLine($"{fieldName} = (({requestTypeName}) body{NullableReferenceBang}).{fieldName},");
													}
												}
											}
										}
									}

									code.WriteLine("ResponseMappings =");
									using (code.Block("{", "},"))
									{
										foreach (var validResponse in httpMethodInfo.ValidResponses)
										{
											code.WriteLine($"new HttpResponseMapping<{responseTypeName}>.Builder");
											using (code.Block("{", "}.Build(),"))
											{
												string statusCode = ((int) validResponse.StatusCode).ToString(CultureInfo.InvariantCulture);
												code.WriteLine($"StatusCode = (HttpStatusCode) {statusCode},");

												var bodyField = validResponse.BodyField;
												if (bodyField != null)
												{
													string responseBodyFieldName = context.GetFieldPropertyName(bodyField.ServiceField);

													var bodyFieldType = context.GetFieldType(bodyField.ServiceField);
													if (bodyFieldType.Kind == ServiceTypeKind.Boolean)
													{
														code.WriteLine($"MatchesResponse = response => response.{responseBodyFieldName}.GetValueOrDefault(),");
														code.WriteLine($"CreateResponse = body => new {responseTypeName} {{ {responseBodyFieldName} = true }},");
													}
													else
													{
														var responseBodyFieldTypeName = RenderNullableFieldType(bodyFieldType);
														var responseNullableBodyFieldTypeName = RenderNullableReferenceFieldType(bodyFieldType);
														code.WriteLine($"ResponseBodyType = typeof({responseBodyFieldTypeName}),");
														if (bodyField.ContentType != null)
															code.WriteLine($"ResponseBodyContentType = {CSharpUtility.CreateString(bodyField.ContentType)},");
														code.WriteLine($"MatchesResponse = response => response.{responseBodyFieldName} != null,");
														code.WriteLine($"GetResponseBody = response => response.{responseBodyFieldName},");
														code.WriteLine($"CreateResponse = body => new {responseTypeName} {{ {responseBodyFieldName} = ({responseNullableBodyFieldTypeName}) body }},");
													}
												}
												else if (validResponse.NormalFields!.Count != 0)
												{
													code.WriteLine($"ResponseBodyType = typeof({responseTypeName}),");

													// copy fields if necessary; full response is the default
													if (httpMethodInfo.ServiceMethod.ResponseFields.Count != validResponse.NormalFields.Count)
													{
														code.WriteLine("GetResponseBody = response =>");
														using (code.Indent())
														{
															code.WriteLine($"new {responseTypeName}");
															using (code.Block("{", "},"))
															{
																foreach (var field in validResponse.NormalFields)
																{
																	string fieldName = context.GetFieldPropertyName(field.ServiceField);
																	code.WriteLine($"{fieldName} = response.{fieldName},");
																}
															}
														}

														code.WriteLine("CreateResponse = body =>");
														using (code.Indent())
														{
															code.WriteLine($"new {responseTypeName}");
															using (code.Block("{", "},"))
															{
																foreach (var field in validResponse.NormalFields)
																{
																	string fieldName = context.GetFieldPropertyName(field.ServiceField);
																	code.WriteLine($"{fieldName} = (({responseTypeName}) body{NullableReferenceBang}).{fieldName},");
																}
															}
														}
													}
												}
											}
										}
									}

									if (httpMethodInfo.ResponseHeaderFields.Count != 0)
									{
										code.WriteLine("GetResponseHeaders = response =>");
										using (code.Indent())
										{
											code.WriteLine($"new Dictionary<string, string{NullableReferenceSuffix}>");
											using (code.Block("{", "},"))
											{
												foreach (var headerField in httpMethodInfo.ResponseHeaderFields)
												{
													string fieldName = context.GetFieldPropertyName(headerField.ServiceField);
													string fieldValue = GenerateFieldToStringCode(context.GetFieldType(headerField.ServiceField), $"response.{fieldName}");
													code.WriteLine($"[\"{headerField.Name}\"] = {fieldValue},");
												}
											}
										}

										code.WriteLine("SetResponseHeaders = (response, headers) =>");
										using (code.Block("{", "},"))
										{
											foreach (var headerField in httpMethodInfo.ResponseHeaderFields)
											{
												string dtoFieldName = context.GetFieldPropertyName(headerField.ServiceField);
												string headerVariableName = $"header{dtoFieldName}";
												code.WriteLine($"headers.TryGetValue(\"{headerField.Name}\", out var {headerVariableName});");
												code.WriteLine($"response.{dtoFieldName} = {GenerateStringToFieldCode(context.GetFieldType(headerField.ServiceField), headerVariableName)};");
											}

											code.WriteLine("return response;");
										}
									}
								}
							}
						}
					}
				}
			});
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
				case ServiceTypeKind.Decimal:
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
					string enumName = CSharpUtility.GetEnumName(serviceType.Enum!);
					return $"{fieldCode} == null ? default({enumName}?) : new {enumName}({fieldCode})";
				case ServiceTypeKind.Boolean:
					return $"ServiceDataUtility.TryParseBoolean({fieldCode})";
				case ServiceTypeKind.Double:
					return $"ServiceDataUtility.TryParseDouble({fieldCode})";
				case ServiceTypeKind.Int32:
					return $"ServiceDataUtility.TryParseInt32({fieldCode})";
				case ServiceTypeKind.Int64:
					return $"ServiceDataUtility.TryParseInt64({fieldCode})";
				case ServiceTypeKind.Decimal:
					return $"ServiceDataUtility.TryParseDecimal({fieldCode})";
				case ServiceTypeKind.String:
					return fieldCode;
				default:
					throw new NotSupportedException("Unexpected field type: " + serviceType.Kind);
			}
		}

		private CodeGenFile GenerateHttpClient(HttpServiceInfo httpServiceInfo, Context context)
		{
			var serviceInfo = httpServiceInfo.Service;

			string namespaceName = $"{context.NamespaceName}.{CSharpUtility.HttpDirectoryName}";
			string fullServiceName = serviceInfo.Name;
			string fullHttpClientName = "HttpClient" + fullServiceName;
			string fullInterfaceName = CSharpUtility.GetInterfaceName(serviceInfo);
			string httpMappingName = serviceInfo.Name + "HttpMapping";

			return CreateFile($"{CSharpUtility.HttpDirectoryName}/{fullHttpClientName}{CSharpUtility.FileExtension}", code =>
			{
				WriteFileHeader(code, context);

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

					code.WriteLine($"public sealed partial class {fullHttpClientName} : HttpClientService, {fullInterfaceName}");
					using (code.Block())
					{
						CSharpUtility.WriteSummary(code, "Creates the service.");
						code.WriteLine($"public {fullHttpClientName}(HttpClientServiceSettings{NullableReferenceSuffix} settings = null)");
						using (code.Indent())
						{
							var url = httpServiceInfo.Url;
							string urlCode = url != null ? $"new Uri({CSharpUtility.CreateString(url)})" : "null";
							code.WriteLine($": base(settings, defaultBaseUri: {urlCode})");
						}
						code.Block().Dispose();

						foreach (HttpMethodInfo httpMethodInfo in httpServiceInfo.Methods)
						{
							var methodInfo = httpMethodInfo.ServiceMethod;
							string methodName = CSharpUtility.GetMethodName(methodInfo);
							string requestTypeName = CSharpUtility.GetRequestDtoName(methodInfo);
							string responseTypeName = CSharpUtility.GetResponseDtoName(methodInfo);

							code.WriteLine();
							CSharpUtility.WriteSummary(code, methodInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, methodInfo);
							code.WriteLine($"public Task<ServiceResult<{responseTypeName}>> {methodName}Async({requestTypeName} request, CancellationToken cancellationToken = default) =>");
							using (code.Indent())
								code.WriteLine($"TrySendRequestAsync({httpMappingName}.{methodName}Mapping, request, cancellationToken);");
						}
					}
				}
			});
		}

		private CodeGenFile GenerateHttpHandler(HttpServiceInfo httpServiceInfo, Context context)
		{
			var serviceInfo = httpServiceInfo.Service;

			string namespaceName = $"{context.NamespaceName}.{CSharpUtility.HttpDirectoryName}";
			string fullServiceName = serviceInfo.Name;
			string fullHttpHandlerName = fullServiceName + "HttpHandler";
			string fullInterfaceName = CSharpUtility.GetInterfaceName(serviceInfo);
			string httpMappingName = serviceInfo.Name + "HttpMapping";

			return CreateFile($"{CSharpUtility.HttpDirectoryName}/{fullHttpHandlerName}{CSharpUtility.FileExtension}", code =>
			{
				WriteFileHeader(code, context);

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

				if (serviceInfo.Methods.Any(x => x.IsObsolete))
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
						code.WriteLine($"public {fullHttpHandlerName}({fullInterfaceName} service, ServiceHttpHandlerSettings{NullableReferenceSuffix} settings = null)");
						using (code.Indent())
							code.WriteLine(": base(settings)");
						using (code.Block())
							code.WriteLine("m_service = service ?? throw new ArgumentNullException(nameof(service));");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Creates the handler.");
						code.WriteLine($"public {fullHttpHandlerName}(Func<HttpRequestMessage, {fullInterfaceName}> getService, ServiceHttpHandlerSettings{NullableReferenceSuffix} settings = null)");
						using (code.Indent())
							code.WriteLine(": base(settings)");
						using (code.Block())
							code.WriteLine("m_getService = getService ?? throw new ArgumentNullException(nameof(getService));");

						code.WriteLine();
						CSharpUtility.WriteSummary(code, "Attempts to handle the HTTP request.");
						code.WriteLine($"public override async Task<HttpResponseMessage{NullableReferenceSuffix}> TryHandleHttpRequestAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken = default)");
						using (code.Block())
						{
							// check 'widgets/get' before 'widgets/{id}'
							IDisposable? indent = null;
							code.Write("return ");
							foreach (var httpServiceMethod in httpServiceInfo.Methods.OrderBy(x => x, HttpMethodInfo.ByRouteComparer))
							{
								if (indent != null)
									code.WriteLine(" ??");
								string methodName = CSharpUtility.GetMethodName(httpServiceMethod.ServiceMethod);
								code.Write($"await AdaptTask(TryHandle{methodName}Async(httpRequest, cancellationToken)).ConfigureAwait(true)");
								indent ??= code.Indent();
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
							code.WriteLine($"public Task<HttpResponseMessage{NullableReferenceSuffix}> TryHandle{methodName}Async(HttpRequestMessage httpRequest, CancellationToken cancellationToken = default) =>");
							using (code.Indent())
								code.WriteLine($"TryHandleServiceMethodAsync({httpMappingName}.{methodName}Mapping, httpRequest, GetService(httpRequest).{methodName}Async, cancellationToken);");
						}

						if (httpServiceInfo.ErrorSets.Count != 0)
						{
							code.WriteLine();
							CSharpUtility.WriteSummary(code, "Returns the HTTP status code for a custom error code.");
							code.WriteLine("protected override HttpStatusCode? TryGetCustomHttpStatusCode(string errorCode) =>");
							using (code.Indent())
							{
								string tryGetCustomHttpStatusCode = string.Join(" ?? ", httpServiceInfo.ErrorSets.Select(x => $"Http{x.ServiceErrorSet.Name}.TryGetHttpStatusCode(errorCode)"));
								code.WriteLine($"{tryGetCustomHttpStatusCode};");
							}
						}

						code.WriteLine();
						code.WriteLine($"private {fullInterfaceName} GetService(HttpRequestMessage httpRequest) => m_service ?? m_getService{NullableReferenceBang}(httpRequest);");

						code.WriteLine();
						code.WriteLine($"readonly {fullInterfaceName}{NullableReferenceSuffix} m_service;");
						code.WriteLine($"readonly Func<HttpRequestMessage, {fullInterfaceName}>{NullableReferenceSuffix} m_getService;");
					}
				}
			});
		}

		private static string? TryGetAreEquivalentMethodName(ServiceTypeKind kind)
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
					return "AreEquivalentFieldValues";
				default:
					return null;
			}
		}

		private IEnumerable<CodeGenFile> GenerateMethodDtos(ServiceMethodInfo methodInfo, Context context)
		{
			yield return GenerateDto(new ServiceDtoInfo(
				name: $"{CodeGenUtility.Capitalize(methodInfo.Name)}Request",
				fields: methodInfo.RequestFields,
				summary: $"Request for {CodeGenUtility.Capitalize(methodInfo.Name)}."), context);

			yield return GenerateDto(new ServiceDtoInfo(
				name: $"{CodeGenUtility.Capitalize(methodInfo.Name)}Response",
				fields: methodInfo.ResponseFields,
				summary: $"Response for {CodeGenUtility.Capitalize(methodInfo.Name)}."), context);
		}

		private void GenerateFieldProperties(CodeWriter code, IEnumerable<ServiceFieldInfo> fieldInfos, Context context)
		{
			foreach (var fieldInfo in fieldInfos)
			{
				string propertyName = context.GetFieldPropertyName(fieldInfo);
				string normalPropertyName = CodeGenUtility.Capitalize(fieldInfo.Name);
				string nullableFieldType = RenderNullableReferenceFieldType(context.GetFieldType(fieldInfo));

				code.WriteLine();
				CSharpUtility.WriteSummary(code, fieldInfo.Summary);
				CSharpUtility.WriteObsoleteAttribute(code, fieldInfo);
				if (propertyName != normalPropertyName)
					code.WriteLine($"[JsonProperty(\"{fieldInfo.Name}\")]");
				code.WriteLine($"public {nullableFieldType} {propertyName} {{ get; set; }}");
			}
		}

		private CodeGenFile GenerateInterface(ServiceInfo serviceInfo, Context context)
		{
			string interfaceName = CSharpUtility.GetInterfaceName(serviceInfo);

			return CreateFile(interfaceName + CSharpUtility.FileExtension, code =>
			{
				WriteFileHeader(code, context);

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
								$"{CSharpUtility.GetRequestDtoName(methodInfo)} request, CancellationToken cancellationToken = default);");
						}
					}
				}
			});
		}

		private CodeGenFile GenerateMethodInfos(ServiceInfo serviceInfo, Context context)
		{
			var className = $"{CodeGenUtility.Capitalize(serviceInfo.Name)}Methods";
			var interfaceName = CSharpUtility.GetInterfaceName(serviceInfo);

			return CreateFile(className + CSharpUtility.FileExtension, code =>
			{
				WriteFileHeader(code, context);

				var usings = new List<string>
				{
					"System",
					"Facility.Core",
				};
				CSharpUtility.WriteUsings(code, usings, context.NamespaceName);

				code.WriteLine($"namespace {context.NamespaceName}");
				using (code.Block())
				{
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, serviceInfo);

					code.WriteLine($"internal static class {className}");
					using (code.Block())
					{
						foreach (var methodInfo in serviceInfo.Methods)
						{
							code.WriteLineSkipOnce();
							CSharpUtility.WriteObsoleteAttribute(code, methodInfo);
							code.WriteLine($"public static readonly IServiceMethodInfo {CSharpUtility.GetMethodName(methodInfo)} =");
							using (code.Indent())
							{
								code.WriteLine($"ServiceMethodInfo.Create<{interfaceName}, {CSharpUtility.GetRequestDtoName(methodInfo)}, {CSharpUtility.GetResponseDtoName(methodInfo)}>(");
								using (code.Indent())
									code.WriteLine($"{CSharpUtility.CreateString(methodInfo.Name)}, {CSharpUtility.CreateString(serviceInfo.Name)}, x => x.{CSharpUtility.GetMethodName(methodInfo)}Async);");
							}
						}
					}
				}
			});
		}

		private CodeGenFile GenerateDelegatingService(ServiceInfo serviceInfo, Context context)
		{
			var className = $"Delegating{CodeGenUtility.Capitalize(serviceInfo.Name)}";
			var interfaceName = CSharpUtility.GetInterfaceName(serviceInfo);
			var methodsClassName = $"{CodeGenUtility.Capitalize(serviceInfo.Name)}Methods";

			return CreateFile(className + CSharpUtility.FileExtension, code =>
			{
				WriteFileHeader(code, context);

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
					CSharpUtility.WriteSummary(code, $"A delegating implementation of {serviceInfo.Name}.");
					CSharpUtility.WriteCodeGenAttribute(code, context.GeneratorName);
					CSharpUtility.WriteObsoleteAttribute(code, serviceInfo);

					code.WriteLine($"public class {className} : {interfaceName}");
					using (code.Block())
					{
						CSharpUtility.WriteSummary(code, "Creates an instance with the specified delegator.");
						code.WriteLine($"public {className}(ServiceDelegator delegator) =>");
						using (code.Indent())
							code.WriteLine("m_delegator = delegator ?? throw new ArgumentNullException(nameof(delegator));");

						foreach (var methodInfo in serviceInfo.Methods)
						{
							code.WriteLine();
							CSharpUtility.WriteSummary(code, methodInfo.Summary);
							CSharpUtility.WriteObsoleteAttribute(code, methodInfo);
							code.WriteLine($"public virtual async Task<ServiceResult<{CSharpUtility.GetResponseDtoName(methodInfo)}>> {CSharpUtility.GetMethodName(methodInfo)}Async({CSharpUtility.GetRequestDtoName(methodInfo)} request, CancellationToken cancellationToken = default) =>");
							using (code.Indent())
								code.WriteLine($"(await m_delegator({methodsClassName}.{CSharpUtility.GetMethodName(methodInfo)}, request, cancellationToken).ConfigureAwait(false)).Cast<{CSharpUtility.GetResponseDtoName(methodInfo)}>();");
						}

						code.WriteLine();
						code.WriteLine("private readonly ServiceDelegator m_delegator;");
					}
				}
			});
		}

		private string RenderNonNullableFieldType(ServiceTypeInfo fieldType) => RenderNullableFieldType(fieldType).TrimEnd('?');

		private string RenderNullableReferenceFieldType(ServiceTypeInfo fieldType) =>
			UseNullableReferences ? (RenderNonNullableFieldType(fieldType) + "?") : RenderNullableFieldType(fieldType);

		private string RenderNullableFieldType(ServiceTypeInfo fieldType)
		{
			return fieldType.Kind switch
			{
				ServiceTypeKind.String => "string",
				ServiceTypeKind.Boolean => "bool?",
				ServiceTypeKind.Double => "double?",
				ServiceTypeKind.Int32 => "int?",
				ServiceTypeKind.Int64 => "long?",
				ServiceTypeKind.Decimal => "decimal?",
				ServiceTypeKind.Bytes => "byte[]",
				ServiceTypeKind.Object => "JObject",
				ServiceTypeKind.Error => "ServiceErrorDto",
				ServiceTypeKind.Dto => CSharpUtility.GetDtoName(fieldType.Dto!),
				ServiceTypeKind.Enum => CSharpUtility.GetEnumName(fieldType.Enum!) + "?",
				ServiceTypeKind.Result => $"ServiceResult<{RenderNonNullableFieldType(fieldType.ValueType!)}>",
				ServiceTypeKind.Array => $"IReadOnlyList<{RenderNonNullableFieldType(fieldType.ValueType!)}>",
				ServiceTypeKind.Map => $"IReadOnlyDictionary<string, {RenderNonNullableFieldType(fieldType.ValueType!)}>",
				_ => throw new NotSupportedException("Unknown field type " + fieldType.Kind),
			};
		}

		private void WriteFileHeader(CodeWriter code, Context context)
		{
			CSharpUtility.WriteFileHeader(code, context.GeneratorName);

			if (UseNullableReferences)
				code.WriteLine("#nullable enable");
		}

		private string NullableReference(string value) => $"{value}{NullableReferenceSuffix}";

		private string NullableReferenceSuffix => UseNullableReferences ? "?" : "";

		private string NullableReferenceBang => UseNullableReferences ? "!" : "";

		private sealed class Context
		{
			public Context(string generatorName, CSharpServiceInfo csharpServiceInfo, string? namespaceName)
			{
				m_csharpServiceInfo = csharpServiceInfo;
				GeneratorName = generatorName;
				NamespaceName = namespaceName ?? m_csharpServiceInfo.Namespace;
				m_dtosNeedingValidation = FindDtosNeedingValidation(m_csharpServiceInfo.Service);
			}

			public string GeneratorName { get; }

			public string NamespaceName { get; }

			public string GetFieldPropertyName(ServiceFieldInfo field) => m_csharpServiceInfo.GetFieldPropertyName(field);

			public ServiceTypeInfo GetFieldType(ServiceFieldInfo field) => m_csharpServiceInfo.Service.GetFieldType(field) ?? throw new InvalidOperationException("Missing field.");

			public bool NeedsValidation(ServiceTypeInfo type) =>
				type.Kind == ServiceTypeKind.Dto && m_dtosNeedingValidation.Contains(type.Dto!) ||
				type.ValueType != null && NeedsValidation(type.ValueType!);

			private static ServiceDtoInfo? TryGetDtoInfo(ServiceTypeInfo? type) =>
				type is null ? null : type.Kind == ServiceTypeKind.Dto ? type.Dto! : TryGetDtoInfo(type.ValueType);

			private static HashSet<ServiceDtoInfo> FindDtosNeedingValidation(ServiceInfo service)
			{
				var dtosNeedingValidation = new HashSet<ServiceDtoInfo>();

				var addedDto = false;
				foreach (var dto in service.Dtos)
				{
					if (dto.Fields.Any(x => x.IsRequired || x.Validation != null))
					{
						dtosNeedingValidation.Add(dto);
						addedDto = true;
					}
				}

				while (addedDto)
				{
					addedDto = false;

					foreach (var dto in service.Dtos.Where(x => !dtosNeedingValidation.Contains(x)))
					{
						foreach (var field in dto.Fields)
						{
							if (TryGetDtoInfo(service.GetFieldType(field)) is ServiceDtoInfo fieldDto && dtosNeedingValidation.Contains(fieldDto))
							{
								dtosNeedingValidation.Add(dto);
								addedDto = true;
								break;
							}
						}
					}
				}

				return dtosNeedingValidation;
			}

			private readonly CSharpServiceInfo m_csharpServiceInfo;
			private readonly HashSet<ServiceDtoInfo> m_dtosNeedingValidation;
		}
	}
}
