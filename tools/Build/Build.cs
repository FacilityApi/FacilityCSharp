using Faithlife.Build;
using static Faithlife.Build.BuildUtility;
using static Faithlife.Build.DotNetRunner;

return BuildRunner.Execute(args, build =>
{
	var codegen = "fsdgencsharp";

	var gitLogin = new GitLoginInfo("FacilityApiBot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? "");

	var dotNetBuildSettings = new DotNetBuildSettings
	{
		NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
		DocsSettings = new DotNetDocsSettings
		{
			GitLogin = gitLogin,
			GitAuthor = new GitAuthorInfo("FacilityApiBot", "facilityapi@gmail.com"),
			SourceCodeUrl = "https://github.com/FacilityApi/FacilityCSharp/tree/master/src",
			ProjectHasDocs = name => !name.StartsWith("fsdgen", StringComparison.Ordinal) && name != "FacilityConformance",
		},
		PackageSettings = new DotNetPackageSettings
		{
			GitLogin = gitLogin,
			PushTagOnPublish = x => $"nuget.{x.Version}",
		},
	};

	build.AddDotNetTargets(dotNetBuildSettings);

	build.Target("codegen")
		.Describe("Generates code from the FSD")
		.Does(() => CodeGen(verify: false));

	build.Target("verify-codegen")
		.Describe("Ensures the generated code is up-to-date")
		.Does(() => CodeGen(verify: true));

	build.Target("test")
		.DependsOn("verify-codegen");

	build.Target("benchmark")
		.Does(() => RunDotNet("run", "--project", "tests/Facility.Benchmarks", "--configuration", "release"));

	void CodeGen(bool verify)
	{
		RunDotNet("build", "--framework", "net6.0", $"src/{codegen}");

		var verifyOption = verify ? "--verify" : null;

		RunCodeGen("fsd/FacilityCore.fsd", "src/Facility.Core/");
		RunCodeGen("conformance/ConformanceApi.fsd", "src/Facility.ConformanceApi/", "--msgpack");
		RunCodeGen("tools/EdgeCases.fsd", "tools/EdgeCases/", "--msgpack");
		RunCodeGen("tests/Facility.Benchmarks/BenchmarkService.fsd", "tests/Facility.Benchmarks/", "--msgpack");

		void RunCodeGen(params string?[] args) => RunDotNet(new[] { "run", "--no-build", "--project", $"src/{codegen}", "--framework", "net6.0", "--nullable", "--newline", "lf", "--clean", verifyOption }.Concat(args));
	}
});
