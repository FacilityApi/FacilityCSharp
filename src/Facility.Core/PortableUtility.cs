namespace Facility.Core;

internal static class PortableUtility
{
#if !NET9_0_OR_GREATER
	public static Task LoadIntoBufferAsync(this HttpContent httpContent, CancellationToken cancellationToken) => httpContent.LoadIntoBufferAsync();
#endif
}
