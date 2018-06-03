// DO NOT EDIT: generated by fsdgencsharp
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
		public HttpClientEdgeCases(HttpClientServiceSettings settings = null)
			: base(settings, defaultBaseUri: null)
		{
		}

		/// <summary>
		/// An old method.
		/// </summary>
		[Obsolete]
		public Task<ServiceResult<OldMethodResponseDto>> OldMethodAsync(OldMethodRequestDto request, CancellationToken cancellationToken) =>
			TrySendRequestAsync(EdgeCasesHttpMapping.OldMethodMapping, request, cancellationToken);
	}
}
