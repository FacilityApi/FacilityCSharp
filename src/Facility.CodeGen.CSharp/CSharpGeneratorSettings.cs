using Facility.Definition.CodeGen;

namespace Facility.CodeGen.CSharp
{
	/// <summary>
	/// Settings for generating C#.
	/// </summary>
	public sealed class CSharpGeneratorSettings : FileGeneratorSettings
	{
		/// <summary>
		/// The name of the namespace (optional).
		/// </summary>
		public string? NamespaceName { get; set; }
	}
}
