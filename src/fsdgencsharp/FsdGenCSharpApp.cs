using ArgsReading;
using Facility.CodeGen.Console;
using Facility.CodeGen.CSharp;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;

namespace fsdgencsharp;

internal sealed class FsdGenCSharpApp : CodeGeneratorApp
{
	public static int Main(string[] args) => new FsdGenCSharpApp().Run(args);

	protected override IReadOnlyList<string> Description =>
	[
		"Generates C# for a Facility Service Definition.",
	];

	protected override IReadOnlyList<string> ExtraUsage =>
	[
		"   --namespace <name>",
		"      The namespace used by the generated C#. (Overrides FSD.)",
		"   --default-namespace <name>",
		"      The namespace used by the generated C# if not specified by FSD.",
		"   --nullable",
		"      Use nullable reference syntax in the generated C#.",
		"   --compress-requests",
		"      Compress HTTP requests by default in the generated C#.",
		"   --fix-snake-case",
		"      Replace snake_case with PascalCase.",
		"   --msgpack",
		"      Support MessagePack serialization.",
		"   --json-source-gen",
		"      Support JSON source generation.",
		"   --json-source-gen-if <condition>",
		"      The #if condition to use around generated JSON source.",
		"   --no-http",
		"      Omit HTTP code generation.",
	];

	protected override ServiceParser CreateParser() => new FsdParser(new FsdParserSettings { SupportsEvents = true });

	protected override CodeGenerator CreateGenerator() => new CSharpGenerator();

	protected override FileGeneratorSettings CreateSettings(ArgsReader args) =>
		new CSharpGeneratorSettings
		{
			NamespaceName = args.ReadOption("namespace"),
			DefaultNamespaceName = args.ReadOption("default-namespace"),
			UseNullableReferences = args.ReadFlag("nullable"),
			CompressRequests = args.ReadFlag("compress-requests"),
			FixSnakeCase = args.ReadFlag("fix-snake-case"),
			SupportMessagePack = args.ReadFlag("msgpack"),
			SupportJsonSourceGeneration = args.ReadFlag("json-source-gen"),
			JsonSourceGenerationCondition = args.ReadOption("json-source-gen-if"),
			NoHttp = args.ReadFlag("no-http"),
		};
}
