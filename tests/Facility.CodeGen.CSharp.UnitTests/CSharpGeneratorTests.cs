using System;
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
	}
}
