using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;
using NUnit.Framework;

namespace Facility.CodeGen.CSharp.UnitTests;

internal sealed class ValidationTests
{
	[Test]
	public void RequestDtoGeneratesValidation()
	{
		var definition = "[csharp] service TestApi { method do { [validate(value: 0..1)] normalized: double; }: {} }";
		var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

		Assert.That(requestDtoFile.Text, Does.Contain("public override bool Validate"));
	}

	[Test]
	public void DataDtoGeneratesRangeValidation()
	{
		var definition = "[csharp] service TestApi { method do { data: Data; }: {} data Data { [validate(value: 0..1)] normalized: double; } }";
		var requestDtoFile = GetGeneratedFile(definition, "DataDto.g.cs");

		Assert.That(requestDtoFile.Text, Does.Contain("public override bool Validate"));
	}

	[Test]
	public void GeneratesNumericValidation()
	{
		var definition = "[csharp] service TestApi { method do { [validate(value: 0..1)] normalized: double; }: {} }";
		var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

		Assert.That(requestDtoFile.Text, Does.Contain("if (Normalized != null && Normalized < 0D)"));
		Assert.That(requestDtoFile.Text, Does.Contain("if (Normalized != null && Normalized > 1D)"));
	}

	[Test]
	public void GeneratesEnumValidation()
	{
		var definition = "[csharp] service TestApi { method do { [validate] data: Data; }: {} enum Data { nothing, something } }";
		var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

		Assert.That(requestDtoFile.Text, Does.Contain("public override bool Validate"));
		Assert.That(requestDtoFile.Text, Does.Contain("if (Data != null && !Data.Value.IsDefined())"));
	}

	[Test]
	public void GeneratesStringLength()
	{
		var definition = "[csharp] service TestApi { method do { [validate(length: 10..)] password: string; }: {} }";
		var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

		Assert.That(requestDtoFile.Text, Does.Not.Contain("using System.Text.RegularExpressions;"));
		Assert.That(requestDtoFile.Text, Does.Contain("if (Password != null && Password.Length < 10)"));
	}

	[Test]
	public void GeneratesStringPattern()
	{
		var definition = """[csharp] service TestApi { method do { [validate(regex: "^[0-9]{4}$")] pin: string; }: {} }""";
		var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

		Assert.That(requestDtoFile.Text, Does.Contain("using System.Text.RegularExpressions;"));
		Assert.That(requestDtoFile.Text, Does.Contain("static readonly Regex s_validPinRegex = new Regex(\"^[0-9]{4}$\", RegexOptions.CultureInvariant);"));
		Assert.That(requestDtoFile.Text, Does.Contain("if (Pin != null && !s_validPinRegex.IsMatch(Pin))"));
	}

	[Test]
	public void GeneratesCollectionCount()
	{
		var definition = "[csharp] service TestApi { method do { [validate(count: ..100)] accountIds: int64[]; }: {} }";
		var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

		Assert.That(requestDtoFile.Text, Does.Contain("if (AccountIds != null && AccountIds.Count > 100"));
	}

	private CodeGenFile GetGeneratedFile(string definition, string fileName)
	{
		var parser = new FsdParser(new FsdParserSettings { SupportsEvents = true });
		var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
		var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };
		var output = generator.GenerateOutput(service);
		return output.Files.First(x => x.Name == fileName);
	}
}
