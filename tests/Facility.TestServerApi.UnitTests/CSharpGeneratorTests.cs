using System.IO;
using System.Linq;
using System.Reflection;
using Facility.CodeGen.CSharp;
using Facility.Definition;
using Facility.Definition.Fsd;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.TestServerApi.UnitTests
{
	public sealed class CSharpGeneratorTests
	{
		[Test]
		public void GenerateTestServerApi()
		{
			ServiceInfo service;
			const string fileName = "Facility.TestServerApi.UnitTests.TestServerApi.fsd";
			var parser = new FsdParser();
			using (var reader = new StreamReader(GetType().GetTypeInfo().Assembly.GetManifestResourceStream(fileName)))
				service = parser.ParseDefinition(new ServiceDefinitionText(Path.GetFileName(fileName), reader.ReadToEnd()));

			var generator = new CSharpGenerator
			{
				GeneratorName = "CSharpGeneratorTests",
			};
			var output = generator.GenerateOutput(service);
			output.Files.Count(x => x.Name == "ITestServerApi.g.cs").Should().Be(1);
		}
	}
}
