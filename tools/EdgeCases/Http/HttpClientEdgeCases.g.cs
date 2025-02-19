// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Http;

namespace EdgeCases.Http
{
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public sealed partial class HttpClientEdgeCases : HttpClientService, IEdgeCases
	{
		/// <summary>
		/// Creates the service.
		/// </summary>
		public HttpClientEdgeCases(HttpClientServiceSettings? settings = null)
			: base(settings, s_defaults)
		{
		}

		/// <summary>
		/// An old method.
		/// </summary>
		[Obsolete]
		public Task<ServiceResult<OldMethodResponseDto>> OldMethodAsync(OldMethodRequestDto request, CancellationToken cancellationToken = default) =>
			TrySendRequestAsync(EdgeCasesHttpMapping.OldMethodMapping, request, cancellationToken);

		/// <summary>
		/// Custom HTTP method.
		/// </summary>
		public Task<ServiceResult<CustomHttpResponseDto>> CustomHttpAsync(CustomHttpRequestDto request, CancellationToken cancellationToken = default) =>
			TrySendRequestAsync(EdgeCasesHttpMapping.CustomHttpMapping, request, cancellationToken);

		public Task<ServiceResult<SnakeMethodResponseDto>> SnakeMethodAsync(SnakeMethodRequestDto request, CancellationToken cancellationToken = default) =>
			TrySendRequestAsync(EdgeCasesHttpMapping.SnakeMethodMapping, request, cancellationToken);

		public Task<ServiceResult<MiscResponseDto>> MiscAsync(MiscRequestDto request, CancellationToken cancellationToken = default) =>
			TrySendRequestAsync(EdgeCasesHttpMapping.MiscMapping, request, cancellationToken);

		private static readonly HttpClientServiceDefaults s_defaults = new HttpClientServiceDefaults
		{
#if NET8_0_OR_GREATER
			ContentSerializer = HttpContentSerializer.Create(EdgeCasesJsonServiceSerializer.Instance),
#else
			ContentSerializer = HttpContentSerializer.Create(SystemTextJsonServiceSerializer.Instance),
#endif
		};
	}
}
