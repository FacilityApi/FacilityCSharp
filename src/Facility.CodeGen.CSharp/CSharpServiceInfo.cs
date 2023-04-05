using Facility.Definition;
using Facility.Definition.CodeGen;

namespace Facility.CodeGen.CSharp;

/// <summary>
/// Service information used when generating C#.
/// </summary>
public sealed class CSharpServiceInfo
{
	/// <summary>
	/// Creates C# info for a service.
	/// </summary>
	/// <exception cref="ServiceDefinitionException">Thrown if there are errors.</exception>
	public static CSharpServiceInfo Create(ServiceInfo serviceInfo) =>
		Create(serviceInfo, settings: null);

	/// <summary>
	/// Creates C# info for a service.
	/// </summary>
	/// <exception cref="ServiceDefinitionException">Thrown if there are errors.</exception>
	public static CSharpServiceInfo Create(ServiceInfo serviceInfo, CSharpServiceInfoSettings? settings) =>
		TryCreate(serviceInfo, settings, out var service, out var errors) ? service : throw new ServiceDefinitionException(errors);

	/// <summary>
	/// Attempts to create C# info for a service.
	/// </summary>
	/// <returns>True if there are no errors.</returns>
	/// <remarks>Even if there are errors, an invalid HTTP mapping will be returned.</remarks>
	public static bool TryCreate(ServiceInfo serviceInfo, out CSharpServiceInfo csharpServiceInfo, out IReadOnlyList<ServiceDefinitionError> errors)
		=> TryCreate(serviceInfo, settings: null, out csharpServiceInfo, out errors);

	/// <summary>
	/// Attempts to create C# info for a service.
	/// </summary>
	/// <returns>True if there are no errors.</returns>
	/// <remarks>Even if there are errors, an invalid HTTP mapping will be returned.</remarks>
	public static bool TryCreate(ServiceInfo serviceInfo, CSharpServiceInfoSettings? settings, out CSharpServiceInfo csharpServiceInfo, out IReadOnlyList<ServiceDefinitionError> errors)
	{
		csharpServiceInfo = new CSharpServiceInfo(serviceInfo, settings, out errors);
		return errors.Count == 0;
	}

	/// <summary>
	/// The service.
	/// </summary>
	public ServiceInfo Service { get; }

	/// <summary>
	/// The namespace.
	/// </summary>
	public string Namespace => m_namespace ?? CodeGenUtility.Capitalize(Service.Name);

	/// <summary>
	/// Gets the property name for the specified field.
	/// </summary>
	public string GetFieldPropertyName(ServiceFieldInfo field) =>
		m_fieldPropertyNames.TryGetValue(field, out var value) ? value : FixName(field.Name);

	internal string GetServiceName(ServiceInfo serviceInfo) => FixName(serviceInfo.Name);

	internal string GetInterfaceName(ServiceInfo serviceInfo) => $"I{FixName(serviceInfo.Name)}";

	internal string GetMethodName(ServiceMethodInfo methodInfo) => FixName(methodInfo.Name);

	internal string GetDtoName(ServiceDtoInfo dtoInfo) => FixName(dtoInfo.Name) + "Dto";

	internal string GetRequestName(ServiceMethodInfo methodInfo) => FixName(methodInfo.Name) + "Request";

	internal string GetResponseName(ServiceMethodInfo methodInfo) => FixName(methodInfo.Name) + "Response";

	internal string GetRequestDtoName(ServiceMethodInfo methodInfo) => FixName(methodInfo.Name) + "RequestDto";

	internal string GetResponseDtoName(ServiceMethodInfo methodInfo) => FixName(methodInfo.Name) + "ResponseDto";

	internal string GetEnumName(ServiceEnumInfo enumInfo) => FixName(enumInfo.Name);

	internal string GetEnumValueName(ServiceEnumValueInfo enumValue) => FixName(enumValue.Name);

	internal string GetErrorSetName(ServiceErrorSetInfo errorSetInfo) => FixName(errorSetInfo.Name);

	internal string GetErrorName(ServiceErrorInfo errorInfo) => FixName(errorInfo.Name);

	internal string GetExternalDtoName(ServiceExternalDtoInfo info)
	{
		var typeName = m_externalDtoNames.TryGetValue(info, out var name) ? name : $"{FixName(info.Name)}Dto";
		var typeNamespace = m_externalDtoNamespaces.TryGetValue(info, out var ns) ? ns : "";
		return typeNamespace.Length != 0 ? $"{typeNamespace}.{typeName}" : typeName;
	}

	internal string GetExternalEnumName(ServiceExternalEnumInfo info)
	{
		var typeName = m_externalEnumNames.TryGetValue(info, out var name) ? name : FixName(info.Name);
		var typeNamespace = m_externalEnumNamespaces.TryGetValue(info, out var ns) ? ns : "";
		return typeNamespace.Length != 0 ? $"{typeNamespace}.{typeName}" : typeName;
	}

	private string FixName(string name) =>
		m_fixSnakeCase && name.ContainsOrdinal('_') ? CodeGenUtility.ToPascalCase(name) : CodeGenUtility.Capitalize(name);

	private CSharpServiceInfo(ServiceInfo serviceInfo, CSharpServiceInfoSettings? settings, out IReadOnlyList<ServiceDefinitionError> errors)
	{
		Service = serviceInfo;
		m_externalDtoNames = new Dictionary<ServiceExternalDtoInfo, string>();
		m_externalDtoNamespaces = new Dictionary<ServiceExternalDtoInfo, string>();
		m_externalEnumNames = new Dictionary<ServiceExternalEnumInfo, string>();
		m_externalEnumNamespaces = new Dictionary<ServiceExternalEnumInfo, string>();
		m_fieldPropertyNames = new Dictionary<ServiceFieldInfo, string>();
		m_fixSnakeCase = settings?.FixSnakeCase ?? false;

		var validationErrors = new List<ServiceDefinitionError>();

		foreach (var descendant in serviceInfo.GetElementAndDescendants().OfType<ServiceElementWithAttributesInfo>())
		{
			var csharpAttributes = descendant.GetAttributes("csharp");
			if (csharpAttributes.Count == 1)
			{
				var csharpAttribute = csharpAttributes[0];
				if (descendant is ServiceInfo || descendant is ServiceFieldInfo)
				{
					foreach (var parameter in csharpAttribute.Parameters)
					{
						if (parameter.Name == "namespace" && descendant is ServiceInfo)
							m_namespace = parameter.Value;
						else if (parameter.Name == "name" && descendant is ServiceFieldInfo field)
							m_fieldPropertyNames[field] = parameter.Value;
						else
							validationErrors.Add(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(csharpAttribute.Name, parameter));
					}
				}
				else if (descendant is ServiceExternalDtoInfo externalDtoInfo)
				{
					foreach (var parameter in csharpAttribute.Parameters)
					{
						if (parameter.Name == "namespace")
							m_externalDtoNamespaces[externalDtoInfo] = parameter.Value;
						else if (parameter.Name == "name")
							m_externalDtoNames[externalDtoInfo] = parameter.Value;
						else
							validationErrors.Add(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(csharpAttribute.Name, parameter));
					}
				}
				else if (descendant is ServiceExternalEnumInfo externalEnumInfo)
				{
					foreach (var parameter in csharpAttribute.Parameters)
					{
						if (parameter.Name == "namespace")
							m_externalEnumNamespaces[externalEnumInfo] = parameter.Value;
						else if (parameter.Name == "name")
							m_externalEnumNames[externalEnumInfo] = parameter.Value;
						else
							validationErrors.Add(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(csharpAttribute.Name, parameter));
					}
				}
				else
				{
					validationErrors.Add(ServiceDefinitionUtility.CreateUnexpectedAttributeError(csharpAttribute));
				}
			}
			else if (csharpAttributes.Count > 1)
			{
				validationErrors.Add(ServiceDefinitionUtility.CreateDuplicateAttributeError(csharpAttributes[1]));
			}
		}

		var typeName = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { GetInterfaceName(serviceInfo) };

		void CheckTypeName(string name, ServiceDefinitionPosition? position)
		{
			if (!typeName.Add(name))
				validationErrors.Add(new ServiceDefinitionError($"Element generates duplicate C# type '{name}'.", position));
		}

		foreach (var member in serviceInfo.Members)
		{
			if (member is ServiceMethodInfo method)
			{
				CheckTypeName(GetRequestDtoName(method), method.Position);
				CheckTypeName(GetResponseDtoName(method), method.Position);
			}
			else if (member is ServiceDtoInfo dto)
			{
				CheckTypeName(GetDtoName(dto), dto.Position);
			}
			else if (member is ServiceEnumInfo @enum)
			{
				CheckTypeName(GetEnumName(@enum), @enum.Position);
			}
			else if (member is ServiceErrorSetInfo errorSet)
			{
				CheckTypeName(GetErrorSetName(errorSet), errorSet.Position);
			}
			else if (member is ServiceExternalDtoInfo externalDto)
			{
				CheckTypeName(GetExternalDtoName(externalDto), externalDto.Position);
			}
			else if (member is ServiceExternalEnumInfo externalEnum)
			{
				CheckTypeName(GetExternalEnumName(externalEnum), externalEnum.Position);
			}
			else
			{
				throw new InvalidOperationException($"Unknown member type {member.GetType().FullName}");
			}
		}

		errors = validationErrors;
	}

	private readonly string? m_namespace;
	private readonly Dictionary<ServiceExternalDtoInfo, string> m_externalDtoNames;
	private readonly Dictionary<ServiceExternalDtoInfo, string> m_externalDtoNamespaces;
	private readonly Dictionary<ServiceExternalEnumInfo, string> m_externalEnumNames;
	private readonly Dictionary<ServiceExternalEnumInfo, string> m_externalEnumNamespaces;
	private readonly Dictionary<ServiceFieldInfo, string> m_fieldPropertyNames;
	private readonly bool m_fixSnakeCase;
}
