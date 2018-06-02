using System.IO;
using System.Linq;
using Facility.CodeGen.CSharp;
using Facility.Definition;
using Facility.Definition.Fsd;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests
{
	public sealed class CodeGenTests
	{
		[Test]
		public void GenerateConformanceApi()
		{
			string fileName = Path.Combine(TestUtility.GetSolutionDirectory(), "conformance", "ConformanceApi.fsd");
			var parser = new FsdParser();
			var service = parser.ParseDefinition(
				new ServiceDefinitionText(Path.GetFileName(fileName), File.ReadAllText(fileName)));

			var generator = new CSharpGenerator { GeneratorName = "CodeGenTests" };
			var output = generator.GenerateOutput(service);
			output.Files.Count(x => x.Name == "IConformanceApi.g.cs").Should().Be(1);
		}
	}
}
