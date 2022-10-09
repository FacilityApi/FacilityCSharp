namespace Facility.CodeGen.CSharp;

/// <summary>
/// Settings for generating C# service info.
/// </summary>
public sealed class CSharpServiceInfoSettings
{
	/// <summary>
	/// True if C# names should automatically use PascalCase instead of snake case.
	/// </summary>
	public bool FixSnakeCase { get; set; }
}
