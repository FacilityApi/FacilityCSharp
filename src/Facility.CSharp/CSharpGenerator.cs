using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Facility.Definition;
using Facility.Definition.CodeGen;

namespace Facility.CSharp
{
	public sealed class CSharpGenerator
	{
		public string GeneratorName { get; set; }

		public string NamespaceName { get; set; }

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
				CSharpUtility.WriteUsings(code, usings, context.GeneratorName);

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
						var fieldInfos = dtoInfo.Fields;
						GenerateFieldProperties(code, fieldInfos, context);

						code.WriteLineSkipOnce();
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

				code.WriteLineSkipOnce();
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
