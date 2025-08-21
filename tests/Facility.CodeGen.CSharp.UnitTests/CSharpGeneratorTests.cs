using Facility.Definition;
using Facility.Definition.Fsd;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.CodeGen.CSharp.UnitTests;

internal sealed class CSharpGeneratorTests
{
	[Test]
	public void DuplicateType()
	{
		ThrowsServiceDefinitionException(
			"service TestApi { method do {}: {} data doRequest {} }",
			"TestApi.fsd(1,36): Element generates duplicate C# type 'DoRequestDto'.");
	}

	[Test]
	public void DuplicateServiceAttribute()
	{
		ThrowsServiceDefinitionException(
			"[csharp] [csharp] service TestApi { method do {}: {} }",
			"TestApi.fsd(1,11): 'csharp' attribute is duplicated.");
	}

	[Test]
	public void UnknownServiceAttributeParameter()
	{
		ThrowsServiceDefinitionException(
			"[csharp(name: hmm)] service TestApi { method do {}: {} }",
			"TestApi.fsd(1,9): Unexpected 'csharp' parameter 'name'.");
	}

	[Test]
	public void UnknownMethodAttribute()
	{
		ThrowsServiceDefinitionException(
			"service TestApi { [csharp] method do {}: {} }",
			"TestApi.fsd(1,20): Unexpected 'csharp' attribute.");
	}

	[Test]
	public void UnknownFieldAttributeParameter()
	{
		ThrowsServiceDefinitionException(
			"service TestApi { method do { [csharp(namespace: hmm)] something: string; }: {} }",
			"TestApi.fsd(1,39): Unexpected 'csharp' parameter 'namespace'.");
	}

	[Test]
	public void UnspecifiedServiceNamespace()
	{
		var definition = "service TestApi { method do {}: {} }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };
		var output = generator.GenerateOutput(service);
		foreach (var file in output.Files)
		{
			Assert.That(file.Text, Does.Contain("namespace TestApi"));
		}
	}

	[Test]
	public void DefaultServiceNamespace()
	{
		var definition = "service TestApi { method do {}: {} }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests), DefaultNamespaceName = "DefaultNamespace" };
		var output = generator.GenerateOutput(service);
		foreach (var file in output.Files)
		{
			Assert.That(file.Text, Does.Contain("namespace DefaultNamespace"));
		}
	}

	[Test]
	public void NoOverrideDefaultServiceNamespace()
	{
		var definition = "[csharp(namespace: DefinitionNamespace)] service TestApi { method do {}: {} }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests), DefaultNamespaceName = "OverrideNamespace" };
		var output = generator.GenerateOutput(service);
		foreach (var file in output.Files)
		{
			Assert.That(file.Text, Does.Contain("namespace DefinitionNamespace"));
			Assert.That(file.Text, Does.Not.Contain("OverrideNamespace"));
		}
	}

	[Test]
	public void OverrideServiceNamespace()
	{
		var definition = "[csharp(namespace: DefinitionNamespace)] service TestApi { method do {}: {} }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests), NamespaceName = "OverrideNamespace" };
		var output = generator.GenerateOutput(service);
		foreach (var file in output.Files)
		{
			Assert.That(file.Text, Does.Contain("namespace OverrideNamespace"));
			Assert.That(file.Text, Does.Not.Contain("DefinitionNamespace"));
		}
	}

	[Test]
	public void RespectsNoHttpOption()
	{
		var definition = "service TestApi { method foo {}: {} data Bar { prop: string; } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests), NoHttp = true };
		var output = generator.GenerateOutput(service);

		var generatedFiles = output.Files.Select(x => x.Name).ToList();
		var httpRelatedFiles = new List<string>
		{
			"Http/HttpClientTestApi.g.cs",
			"Http/TestApiHttpHandler.g.cs",
			"Http/TestApiHttpMapping.g.cs",
		};

		foreach (var httpRelatedFile in httpRelatedFiles)
			Assert.That(generatedFiles, Does.Not.Contain(httpRelatedFile));
	}

	[Test]
	public void GenerateEnumStringConstants()
	{
		const string definition = "service TestApi { enum Answer { yes, no, maybe } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };

		var output = generator.GenerateOutput(service);

		var file = output.Files.First(x => x.Name == "Answer.g.cs");
		Assert.That(file.Text, Does.Contain("public static class Strings"));
		Assert.That(file.Text, Does.Contain("public const string Yes = \"yes\";"));
		Assert.That(file.Text, Does.Contain("public const string No = \"no\";"));
		Assert.That(file.Text, Does.Contain("public const string Maybe = \"maybe\";"));
	}

	[Test]
	public void GenerateExternalDtoPropertyWithNamespace()
	{
		const string definition = "service TestApi { [csharp(name: \"ExternThingDto\", namespace: \"Some.Name.Space\")] extern data Thing; data Test { thing: Thing; } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };

		var output = generator.GenerateOutput(service);

		var file = output.Files.First(x => x.Name == "TestDto.g.cs");
		Assert.That(file.Text, Does.Contain("public Some.Name.Space.ExternThingDto Thing { get; set; }"));
		Assert.That(file.Text, Does.Contain("ServiceDataUtility.AreEquivalentDtos(Thing, other.Thing)"));
	}

	[Test]
	public void GenerateExternalDtoPropertyWithoutNamespace()
	{
		const string definition = "service TestApi { [csharp(name: \"ExternThingDto\")] extern data Thing; data Test { thing: Thing; } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };

		var output = generator.GenerateOutput(service);

		var file = output.Files.First(x => x.Name == "TestDto.g.cs");
		Assert.That(file.Text, Does.Contain("public ExternThingDto Thing { get; set; }"));
		Assert.That(file.Text, Does.Contain("ServiceDataUtility.AreEquivalentDtos(Thing, other.Thing)"));
	}

	[Test]
	public void GenerateExternalDtoPropertyWithoutTypeName()
	{
		const string definition = "service TestApi { extern data Thing; data Test { thing: Thing; } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };

		var output = generator.GenerateOutput(service);

		var file = output.Files.First(x => x.Name == "TestDto.g.cs");
		Assert.That(file.Text, Does.Contain("public ThingDto Thing { get; set; }"));
		Assert.That(file.Text, Does.Contain("ServiceDataUtility.AreEquivalentDtos(Thing, other.Thing)"));
	}

	[Test]
	public void GenerateExternalEnumPropertyWithNamespace()
	{
		const string definition = "service TestApi { [csharp(name: \"ExternSomeEnum\", namespace: \"Some.Name.Space\")] extern enum SomeEnum; data Test { thing: SomeEnum; } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };

		var output = generator.GenerateOutput(service);

		var file = output.Files.First(x => x.Name == "TestDto.g.cs");
		Assert.That(file.Text, Does.Contain("public Some.Name.Space.ExternSomeEnum? Thing { get; set; }"));
		Assert.That(file.Text, Does.Contain("Thing == other.Thing"));
	}

	[Test]
	public void GenerateExternalEnumPropertyWithoutNamespace()
	{
		const string definition = "service TestApi { [csharp(name: \"ExternSomeEnum\")] extern enum SomeEnum; data Test { thing: SomeEnum; } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };

		var output = generator.GenerateOutput(service);

		var file = output.Files.First(x => x.Name == "TestDto.g.cs");
		Assert.That(file.Text, Does.Contain("public ExternSomeEnum? Thing { get; set; }"));
		Assert.That(file.Text, Does.Contain("Thing == other.Thing"));
	}

	[Test]
	public void GenerateExternalEnumPropertyWithoutTypeName()
	{
		const string definition = "service TestApi { extern enum SomeEnum; data Test { thing: SomeEnum; } }";
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };

		var output = generator.GenerateOutput(service);

		var file = output.Files.First(x => x.Name == "TestDto.g.cs");
		Assert.That(file.Text, Does.Contain("public SomeEnum? Thing { get; set; }"));
		Assert.That(file.Text, Does.Contain("Thing == other.Thing"));
	}

	private void ThrowsServiceDefinitionException(string definition, string message)
	{
		var parser = CreateParser();
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = "CodeGenTests" };
		Action action = () => generator.GenerateOutput(service);
		action.Should().Throw<ServiceDefinitionException>().WithMessage(message);
	}

	private static FsdParser CreateParser() => new FsdParser(new FsdParserSettings { SupportsEvents = true });
}
