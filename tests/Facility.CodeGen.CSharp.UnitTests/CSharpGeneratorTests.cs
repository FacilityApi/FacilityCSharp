using System;
using System.IO;
using Facility.Definition;
using Facility.Definition.Fsd;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.CodeGen.CSharp.UnitTests
{
	public sealed class CSharpGeneratorTests
	{
		[Test]
		public void DuplicateType()
		{
			var parser = new FsdParser();
			var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd",
				"service TestApi { method do {}: {} data doRequest {} }"));
			var generator = new CSharpGenerator { GeneratorName = "CodeGenTests" };
			Action action = () => generator.GenerateOutput(service);
			action.Should().Throw<ServiceDefinitionException>()
				.WithMessage("TestApi.fsd(1,36): Element generates duplicate C# type 'DoRequestDto'.");
		}

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
