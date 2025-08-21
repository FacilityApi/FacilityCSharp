using Facility.Definition.CodeGen;

namespace Facility.CodeGen.CSharp;

/// <summary>
/// Settings for generating C#.
/// </summary>
public sealed class CSharpGeneratorSettings : FileGeneratorSettings
{
	/// <summary>
	/// The name of the namespace (optional). Overrides the csharp FSD attribute.
	/// </summary>
	public string? NamespaceName { get; set; }

	/// <summary>
	/// The default name of the namespace (optional). Does not override the csharp FSD attribute.
	/// </summary>
	public string? DefaultNamespaceName { get; set; }

	/// <summary>
	/// True if the code should use nullable reference syntax.
	/// </summary>
	public bool UseNullableReferences { get; set; }

	/// <summary>
	/// True if the code should compress HTTP requests.
	/// </summary>
	public bool CompressRequests { get; set; }

	/// <summary>
	/// True if C# names should automatically use PascalCase instead of snake case.
	/// </summary>
	public bool FixSnakeCase { get; set; }

	/// <summary>
	/// True to support MessagePack serialization.
	/// </summary>
	public bool SupportMessagePack { get; set; }

	/// <summary>
	/// True to support <c>System.Text.Json</c> source generation.
	/// </summary>
	/// <remarks>Unless <see cref="JsonSourceGenerationCondition"/> is set, the
	/// corresponding code is surrounded by <c>#if NET8_0_OR_GREATER</c>.</remarks>
	public bool SupportJsonSourceGeneration { get; set; }

	/// <summary>
	/// The <c>#if</c> condition used around the source generated for <c>System.Text.Json</c>.
	/// </summary>
	/// <remarks>Use <c>true</c> to omit the <c>#if</c>.</remarks>
	public string? JsonSourceGenerationCondition { get; set; }

	/// <summary>
	/// True if HTTP documentation should be omitted.
	/// </summary>
	public bool NoHttp { get; set; }
}
