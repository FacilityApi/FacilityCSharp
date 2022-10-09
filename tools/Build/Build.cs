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
		.DependsOn("build")
		.Describe("Generates code from the FSD")
		.Does(() => CodeGen(verify: false));

	build.Target("verify-codegen")
		.DependsOn("build")
		.Describe("Ensures the generated code is up-to-date")
		.Does(() => CodeGen(verify: true));

	build.Target("test")
		.DependsOn("verify-codegen");

	build.Target("benchmark")
		.Describe("Run benchmarks.")
		.Does(() => RunDotNet("run", "--project", "tests/Facility.Benchmarks", "-c", "Release"));

	void CodeGen(bool verify)
	{
		var configuration = dotNetBuildSettings.GetConfiguration();
		var verifyOption = verify ? "--verify" : null;

		RunCodeGen("fsd/FacilityCore.fsd", "src/Facility.Core/", "--nullable");
		RunCodeGen("conformance/ConformanceApi.fsd", "src/Facility.ConformanceApi/", "--msgpack", "--nullable", "--clean");
		RunCodeGen("tools/EdgeCases.fsd", "tools/EdgeCases/", "--msgpack", "--nullable", "--fix-snake-case", "--clean");
		RunCodeGen("tests/Facility.Benchmarks/BenchmarkService.fsd", "tests/Facility.Benchmarks/", "--msgpack", "--nullable");

		void RunCodeGen(params string?[] args) =>
			RunDotNet(new[] { "run", "--no-build", "--project", $"src/{codegen}", "-f", "net6.0", "-c", configuration, "--", "--newline", "lf", verifyOption }.Concat(args));
	}
});
