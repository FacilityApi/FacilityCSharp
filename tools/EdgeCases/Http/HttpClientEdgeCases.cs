using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;

namespace EdgeCases.Http;

public sealed partial class HttpClientEdgeCases
{
	protected override Task RequestReadyAsync(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
	{
		if (requestDto is CustomHttpRequestDto { Extras: { Count: > 0 } extras })
		{
			httpRequest.RequestUri = new Uri(
				httpRequest.RequestUri!.OriginalString +
				(!httpRequest.RequestUri!.OriginalString.Contains('?', StringComparison.Ordinal) ? "?" : "&") +
				string.Join("&", extras.Select(x => $"extra-{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}")),
				httpRequest.RequestUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
		}

		return Task.CompletedTask;
	}
}
