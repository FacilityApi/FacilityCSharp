using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Facility.Definition;
using Facility.Definition.CodeGen;

namespace Facility.CSharp
{
	public static class CSharpUtility
	{
		public static void WriteFileHeader(CodeWriter code, string generatorName)
		{
			code.WriteLine("// " + CodeGenUtility.GetCodeGenComment(generatorName));
		}

		public static void WriteObsoletePragma(CodeWriter code)
		{
			code.WriteLine("#pragma warning disable 612 // member is obsolete");
		}

		public static void WriteCodeGenAttribute(CodeWriter code, string generatorName)
		{
			code.WriteLine($"[System.CodeDom.Compiler.GeneratedCode(\"{generatorName}\", \"\")]");
		}

		public static void WriteObsoleteAttribute(CodeWriter code, IServiceElementInfo element)
		{
			if (element.IsObsolete())
				code.WriteLine("[Obsolete]");
		}

		public static void WriteUsings(CodeWriter code, IEnumerable<string> namespaceNames, string namespaceName)
		{
			List<string> sortedNamespaceNames = namespaceNames.Distinct().Where(x => namespaceName != x && !namespaceName.StartsWith(x + ".", StringComparison.Ordinal)).ToList();
			sortedNamespaceNames.Sort(CompareUsings);
			if (sortedNamespaceNames.Count != 0)
			{
				foreach (string namepaceName in sortedNamespaceNames)
					code.WriteLine("using " + namepaceName + ";");
				code.WriteLine();
			}
		}

		public static void WriteSummary(CodeWriter code, string summary)
		{
			if (!string.IsNullOrWhiteSpace(summary))
			{
				code.WriteLine("/// <summary>");
				code.WriteLine("/// " + summary);
				code.WriteLine("/// </summary>");
			}
		}

		public const string FileExtension = ".g.cs";

		public const string CommonNamespace = "Common";

		public static string GetNamespaceName(ServiceInfo serviceInfo)
		{
			return CodeGenUtility.Capitalize(serviceInfo.Name);
		}

		public static string GetInterfaceName(ServiceInfo serviceInfo)
		{
			return $"I{CodeGenUtility.Capitalize(serviceInfo.Name)}";
		}

		public static string GetMethodName(ServiceMethodInfo methodInfo)
		{
			return CodeGenUtility.Capitalize(methodInfo.Name);
		}

		public static string GetDtoName(ServiceDtoInfo dtoInfo)
		{
			return CodeGenUtility.Capitalize(dtoInfo.Name) + "Dto";
		}

		public static string GetRequestName(ServiceMethodInfo methodInfo)
		{
			return CodeGenUtility.Capitalize(methodInfo.Name) + "RequestDto";
		}

		public static string GetResponseName(ServiceMethodInfo methodInfo)
		{
			return CodeGenUtility.Capitalize(methodInfo.Name) + "ResponseDto";
		}

		public static string GetFieldPropertyName(ServiceFieldInfo fieldInfo)
		{
			return TryGetCSharpName(fieldInfo) ?? CodeGenUtility.Capitalize(fieldInfo.Name);
		}

		public static string GetEnumName(ServiceEnumInfo enumInfo)
		{
			return CodeGenUtility.Capitalize(enumInfo.Name);
		}

		public static string GetEnumValueName(ServiceEnumValueInfo enumValue)
		{
			return CodeGenUtility.Capitalize(enumValue.Name);
		}

		public static string GetErrorSetName(ServiceErrorSetInfo errorSetInfo)
		{
			return CodeGenUtility.Capitalize(errorSetInfo.Name);
		}

		public static string GetErrorName(ServiceErrorInfo errorInfo)
		{
			return CodeGenUtility.Capitalize(errorInfo.Name);
		}

		public static string CreateString(string text)
		{
			var builder = new StringBuilder(text.Length + 2);
			builder.Append('\"');
			foreach (char ch in text)
			{
				if (ch == '"' || ch == '\\')
					builder.Append('\\').Append(ch);
				else if (ch >= 0x20 && ch <= 0x7E)
					builder.Append(ch);
				else
					builder.Append("\\u").Append(((int) ch).ToString("x4", CultureInfo.InvariantCulture));
			}
			builder.Append('\"');
			return builder.ToString();
		}

		private static int CompareUsings(string left, string right)
		{
			int leftGroup = GetUsingGroup(left);
			int rightGroup = GetUsingGroup(right);
			int result = leftGroup.CompareTo(rightGroup);
			if (result != 0)
				return result;

			return string.CompareOrdinal(left, right);
		}

		private static int GetUsingGroup(string namespaceName)
		{
			return namespaceName == "System" || namespaceName.StartsWith("System.", StringComparison.Ordinal) ? 1 : 2;
		}

		private static string TryGetCSharpName(IServiceElementInfo element)
		{
			return element?.TryGetAttribute("csharp")?.Parameters.SingleOrDefault(x => x.Name == "name")?.Value;
		}
	}
}
