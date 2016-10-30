using System.IO;
using System.Reflection;
using Facility.Definition;
using Facility.Definition.Fsd;
using NUnit.Framework;

namespace Facility.CSharp.UnitTests
{
	public sealed class CSharpGeneratorTests
	{
		[Test]
		public void GenerateExampleApiSuccess()
		{
			ServiceInfo service;
			const string fileName = "Facility.CSharp.UnitTests.ExampleApi.fsd";
			var parser = new FsdParser();
			using (var reader = new StreamReader(GetType().GetTypeInfo().Assembly.GetManifestResourceStream(fileName)))
				service = parser.ParseDefinition(new ServiceTextSource(reader.ReadToEnd()).WithName(Path.GetFileName(fileName)));

			var generator = new CSharpGenerator
			{
				GeneratorName = "CSharpGeneratorTests",
			};
			generator.GenerateOutput(service);
		}
	}
}
