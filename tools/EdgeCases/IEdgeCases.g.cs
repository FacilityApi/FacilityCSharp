// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;

namespace EdgeCases
{
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public partial interface IEdgeCases
	{
		/// <summary>
		/// An old method.
		/// </summary>
		[Obsolete]
		Task<ServiceResult<OldMethodResponseDto>> OldMethodAsync(OldMethodRequestDto request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Custom HTTP method.
		/// </summary>
		Task<ServiceResult<CustomHttpResponseDto>> CustomHttpAsync(CustomHttpRequestDto request, CancellationToken cancellationToken = default);

		Task<ServiceResult<SnakeMethodResponseDto>> SnakeMethodAsync(SnakeMethodRequestDto request, CancellationToken cancellationToken = default);

		Task<ServiceResult<MiscResponseDto>> MiscAsync(MiscRequestDto request, CancellationToken cancellationToken = default);
	}
}
