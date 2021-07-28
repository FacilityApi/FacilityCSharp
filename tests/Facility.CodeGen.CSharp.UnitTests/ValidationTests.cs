using System.Linq;
using Facility.Definition;
using Facility.Definition.CodeGen;
using Facility.Definition.Fsd;
using NUnit.Framework;

namespace Facility.CodeGen.CSharp.UnitTests
{
	public class ValidationTests
	{
		[Test]
		public void RequestDtoGeneratesValidation()
		{
			var definition = "[csharp] service TestApi { method do { [validate(value: 0..1)] normalized: double; }: {} }";
			var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

			StringAssert.Contains("public override bool Validate", requestDtoFile.Text);
		}

		[Test]
		public void DataDtoGeneratesRangeValidation()
		{
			var definition = "[csharp] service TestApi { method do { data: Data; }: {} data Data { [validate(value: 0..1)] normalized: double; } }";
			var requestDtoFile = GetGeneratedFile(definition, "DataDto.g.cs");

			StringAssert.Contains("public override bool Validate", requestDtoFile.Text);
		}

		[Test]
		public void GeneratesNumericValidation()
		{
			var definition = "[csharp] service TestApi { method do { [validate(value: 0..1)] normalized: double; }: {} }";
			var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

			StringAssert.Contains("if (Normalized != null && Normalized < 0D)", requestDtoFile.Text);
			StringAssert.Contains("if (Normalized != null && Normalized > 1D)", requestDtoFile.Text);
		}

		[Test]
		public void GeneratesEnumValidation()
		{
			var definition = "[csharp] service TestApi { method do { [validate] data: Data; }: {} enum Data { nothing, something } }";
			var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

			StringAssert.Contains("public override bool Validate", requestDtoFile.Text);
			StringAssert.Contains("if (Data != null && !Data.Value.IsDefined())", requestDtoFile.Text);
		}

		[Test]
		public void GeneratesStringLength()
		{
			var definition = "[csharp] service TestApi { method do { [validate(length: 10..)] password: string; }: {} }";
			var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

			StringAssert.Contains("if (Password != null && Password.Length < 10)", requestDtoFile.Text);
		}

		[Test]
		public void GeneratesStringPattern()
		{
			var definition = @"[csharp] service TestApi { method do { [validate(regex: ""^[0-9]{4}$"")] pin: string; }: {} }";
			var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

			StringAssert.Contains("static readonly Regex s_ValidPinPattern = new Regex(\"^[0-9]{4}$\");", requestDtoFile.Text);
			StringAssert.Contains("if (Pin != null && !s_ValidPinPattern.IsMatch(Pin))", requestDtoFile.Text);
		}

		[Test]
		public void GeneratesCollectionCount()
		{
			var definition = @"[csharp] service TestApi { method do { [validate(count: ..100)] accountIds: int64[]; }: {} }";
			var requestDtoFile = GetGeneratedFile(definition, "DoRequestDto.g.cs");

			StringAssert.Contains("if (AccountIds != null && AccountIds.Count > 100", requestDtoFile.Text);
		}

		private CodeGenFile GetGeneratedFile(string definition, string fileName)
		{
			var parser = new FsdParser();
			var service = parser.ParseDefinition(new ServiceDefinitionText("TestApi.fsd", definition));
			var generator = new CSharpGenerator { GeneratorName = nameof(CSharpGeneratorTests) };
			var output = generator.GenerateOutput(service);
			return output.Files.First(x => x.Name == fileName);
		}
	}
}
