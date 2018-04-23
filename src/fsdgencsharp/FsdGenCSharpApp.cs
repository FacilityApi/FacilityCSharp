using System;
using System.Collections.Generic;
using System.IO;
using ArgsReading;
using Facility.CodeGen.Console;
using Facility.CodeGen.CSharp;
using Facility.Definition;
using Facility.Definition.CodeGen;

namespace fsdgencsharp
{
	public sealed class FsdGenCSharpApp : CodeGeneratorApp
	{
		public static int Main(string[] args)
		{
			return new FsdGenCSharpApp().Run(args);
		}

		protected override IReadOnlyList<string> Description => new[]
		{
			"Generates C# for a Facility Service Definition.",
		};

		protected override IReadOnlyList<string> ExtraUsage => new[]
		{
			"   --namespace <name>",
			"      The namespace used by the generated C#.",
			"   --csproj",
			"      Update any .csproj files in the output directory.",
		};

		protected override CodeGenerator CreateGenerator(ArgsReader args)
		{
			m_updateCsproj = args.ReadFlag("csproj");

			return new CSharpGenerator
			{
				NamespaceName = args.ReadOption("namespace"),
			};
		}

		protected override void PrepareGenerator(CodeGenerator generator, ServiceInfo service, string outputPath)
		{
			if (m_updateCsproj)
			{
				var csprojFiles = new List<CodeGenFile>();

				var outputDirectoryInfo = new DirectoryInfo(outputPath);
				if (outputDirectoryInfo.Exists)
				{
					foreach (var csprojFileInfo in outputDirectoryInfo.GetFiles("*.csproj"))
						csprojFiles.Add(new CodeGenFile(Path.GetFileName(csprojFileInfo.FullName), File.ReadAllText(csprojFileInfo.FullName)));
				}

				((CSharpGenerator) generator).CsprojFiles = csprojFiles;
			}
		}

		protected override bool ShouldWriteByteOrderMark(string name) => name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);

		protected override bool SupportsClean => true;

		bool m_updateCsproj;
	}
}
