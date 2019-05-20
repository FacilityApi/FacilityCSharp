using System.IO;
using Facility.Definition;
using Facility.Definition.Fsd;
using NUnit.Framework;

namespace Facility.CodeGen.CSharp.UnitTests
{
	public sealed class EdgeCaseTests
	{
		[Test]
		public void GenerateEdgeCases()
		{
			string fileName = Path.Combine(TestUtility.GetSolutionDirectory(), "tools", "EdgeCases.fsd");
			var parser = new FsdParser();
			var service = parser.ParseDefinition(
				new ServiceDefinitionText(Path.GetFileName(fileName), File.ReadAllText(fileName)));

			var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };
			generator.GenerateOutput(service);
		}
	}
}
