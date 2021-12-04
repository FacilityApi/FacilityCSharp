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

	void CodeGen(bool verify)
	{
		var verifyOption = verify ? "--verify" : null;

		RunCodeGen("fsd/FacilityCore.fsd", "src/Facility.Core/", "--nullable", "--newline", "lf", verifyOption);
		RunCodeGen("conformance/ConformanceApi.fsd", "src/Facility.ConformanceApi/", "--nullable", "--newline", "lf", "--clean", verifyOption);
		RunCodeGen("tools/EdgeCases.fsd", "tools/EdgeCases/", "--nullable", "--newline", "lf", "--clean", verifyOption);

		void RunCodeGen(params string?[] args) => RunDotNet(new[] { "run", "--project", $"src/{codegen}", "--framework", "net6.0" }.Concat(args));
	}
});
