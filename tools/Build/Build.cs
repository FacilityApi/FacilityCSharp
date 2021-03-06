using System;
using System.Linq;
using Faithlife.Build;
using static Faithlife.Build.BuildUtility;
using static Faithlife.Build.DotNetRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var codegen = "fsdgencsharp";

		var dotNetBuildSettings = new DotNetBuildSettings
		{
			NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
			DocsSettings = new DotNetDocsSettings
			{
				GitLogin = new GitLoginInfo("FacilityApiBot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
				GitAuthor = new GitAuthorInfo("FacilityApiBot", "facilityapi@gmail.com"),
				SourceCodeUrl = "https://github.com/FacilityApi/FacilityCSharp/tree/master/src",
				ProjectHasDocs = name => name != "fsdgencsharp" && name != "FacilityConformance",
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

		void CodeGen(bool verify)
		{
			var configuration = dotNetBuildSettings!.BuildOptions!.ConfigurationOption!.Value;
			var toolPath = FindFiles($"src/{codegen}/bin/{configuration}/netcoreapp3.1/{codegen}.dll").FirstOrDefault();

			var verifyOption = verify ? "--verify" : null;

			RunDotNet(toolPath, "fsd/FacilityCore.fsd", "src/Facility.Core/", "--nullable", "--newline", "lf", verifyOption);
			RunDotNet(toolPath, "conformance/ConformanceApi.fsd", "src/Facility.ConformanceApi/", "--nullable", "--newline", "lf", "--clean", verifyOption);
			RunDotNet(toolPath, "tools/EdgeCases.fsd", "tools/EdgeCases/", "--nullable", "--newline", "lf", "--clean", verifyOption);
		}
	});
}
