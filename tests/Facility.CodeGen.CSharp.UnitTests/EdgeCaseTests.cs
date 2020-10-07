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
			string fsdText;
			using (var fsdTextReader = new StreamReader(GetType().Assembly.GetManifestResourceStream("Facility.CodeGen.CSharp.UnitTests.EdgeCases.fsd")!))
				fsdText = fsdTextReader.ReadToEnd();

			var parser = new FsdParser();
			var service = parser.ParseDefinition(new ServiceDefinitionText("EdgeCases.fsd", fsdText));

			var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };
			generator.GenerateOutput(service);
		}
	}
}
