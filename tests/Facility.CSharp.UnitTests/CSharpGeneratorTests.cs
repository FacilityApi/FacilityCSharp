using System.IO;
using System.Reflection;
using Facility.Definition;
using Facility.Definition.Fsd;
using Xunit;

namespace Facility.CSharp.UnitTests
{
	public sealed class CSharpGeneratorTests
	{
		[Fact]
		public void GenerateExampleApiSuccess()
		{
			ServiceDefinitionInfo definition;
			const string fileName = "Facility.CSharp.UnitTests.ExampleApi.fsd";
			var parser = new FsdParser();
			using (var reader = new StreamReader(GetType().GetTypeInfo().Assembly.GetManifestResourceStream(fileName)))
				definition = parser.ParseDefinition(new ServiceTextSource(name: Path.GetFileName(fileName), text: reader.ReadToEnd()));

			var generator = new CSharpGenerator
			{
				GeneratorName = "CSharpGeneratorTests",
				NamespaceName = "Facility.ExampleApi"
			};
			generator.GenerateOutput(definition);
		}
	}
}
