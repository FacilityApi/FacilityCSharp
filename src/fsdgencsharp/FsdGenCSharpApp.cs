using ArgsReading;
using Facility.CodeGen.Console;
using Facility.CodeGen.CSharp;
using Facility.Definition.CodeGen;

namespace fsdgencsharp;

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
		"      The namespace used by the generated C#. (Overrides FSD.)",
		"   --default-namespace <name>",
		"      The namespace used by the generated C# if not specified by FSD.",
		"   --nullable",
		"      Use nullable reference syntax in the generated C#.",
		"   --fix-snake-case",
		"      Replace snake_case with PascalCase.",
		"   --msgpack",
		"      Support MessagePack serialization.",
	};

	protected override CodeGenerator CreateGenerator() => new CSharpGenerator();

	protected override FileGeneratorSettings CreateSettings(ArgsReader args) =>
		new CSharpGeneratorSettings
		{
			NamespaceName = args.ReadOption("namespace"),
			DefaultNamespaceName = args.ReadOption("default-namespace"),
			UseNullableReferences = args.ReadFlag("nullable"),
			FixSnakeCase = args.ReadFlag("fix-snake-case"),
			SupportMessagePack = args.ReadFlag("msgpack"),
		};
}
