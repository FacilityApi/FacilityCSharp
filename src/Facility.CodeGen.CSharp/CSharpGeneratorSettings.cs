using Facility.Definition.CodeGen;

namespace Facility.CodeGen.CSharp;

/// <summary>
/// Settings for generating C#.
/// </summary>
public sealed class CSharpGeneratorSettings : FileGeneratorSettings
{
	/// <summary>
	/// The name of the namespace (optional).
	/// </summary>
	public string? NamespaceName { get; set; }

	/// <summary>
	/// True if the code should use nullable reference syntax.
	/// </summary>
	public bool UseNullableReferences { get; set; }

	/// <summary>
	/// True if C# names should automatically use PascalCase instead of snake case.
	/// </summary>
	public bool FixSnakeCase { get; set; }
}
