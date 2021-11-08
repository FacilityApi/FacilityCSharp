using System;
using System.Collections.Generic;
using ArgsReading;
using Facility.CodeGen.Console;
using Facility.CodeGen.CSharp;
using Facility.Definition.CodeGen;

namespace fsdgencsharp
{
	public sealed class FsdGenCSharpApp : CodeGeneratorApp
	{
		public static int Main(string[] args) => new FsdGenCSharpApp().Run(args);

		protected override IReadOnlyList<string> Description => new[]
		{
			"Generates C# for a Facility Service Definition.",
		};

		protected override IReadOnlyList<string> ExtraUsage => new[]
		{
			"   --namespace <name>",
			"      The namespace used by the generated C#.",
			"   --nullable",
			"      Use nullable reference syntax in the generated C#.",
			"   --serializer <serializer>",
			"      Use the specified serializer in the generated C#. Supported values are: NewtonsoftJson",
		};

		protected override CodeGenerator CreateGenerator() => new CSharpGenerator();

		protected override FileGeneratorSettings CreateSettings(ArgsReader args) =>
			new CSharpGeneratorSettings
			{
				NamespaceName = args.ReadOption("namespace"),
				UseNullableReferences = args.ReadFlag("nullable"),
				Serializer = args.ReadOption("serializer") is { } serializerOption ?
					Enum.TryParse<ServiceSerializerKind>(serializerOption, ignoreCase: true, out var serializer) ? serializer : throw new ArgsReaderException("Invalid serializer value") :
					default,
			};
	}
}
