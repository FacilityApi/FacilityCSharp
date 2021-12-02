using System.Net.Http.Headers;
using System.Text;

namespace Facility.Core.Http
{
	/// <summary>
	/// Serializes and deserializes text for HTTP requests and responses.
	/// </summary>
	public class TextHttpContentSerializer : HttpContentSerializer
	{
		/// <summary>
		/// An instance of BytesHttpContentSerializer.
		/// </summary>
		public static readonly TextHttpContentSerializer Instance = new();

		/// <summary>
		/// The default media type for the serializer.
		/// </summary>
		protected override string DefaultMediaTypeCore => "text/plain";

		/// <summary>
		/// Determines if the specified media type is supported.
		/// </summary>
		protected override bool IsSupportedMediaTypeCore(string mediaType) => true;

		/// <summary>
		/// Determines if the specified media type is accepted when investigating the Accept header.
		/// </summary>
		protected override bool IsAcceptedMediaTypeCore(string mediaType) => false;

		/// <summary>
		/// Creates HTTP content for the specified DTO.
		/// </summary>
		protected override HttpContent CreateHttpContentCore(object content, string? mediaType)
		{
			mediaType ??= DefaultMediaTypeCore;

			if (content is string text)
			{
				var httpContent = new StringContent(text, Encoding.UTF8);
				httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
				return httpContent;
			}

			throw new ArgumentException($"Unexpected content type: {content.GetType().Name}", nameof(content));
		}

		/// <summary>
		/// Reads an object from the specified HTTP content.
		/// </summary>
		protected override async Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type objectType, HttpContent content, CancellationToken cancellationToken)
		{
			if (objectType == typeof(string))
			{
#if NET6_0_OR_GREATER
				var stringValue = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
				var stringValue = await content.ReadAsStringAsync().ConfigureAwait(false);
#endif
				return ServiceResult.Success((object) stringValue);
			}

			throw new ArgumentException($"Unexpected content type: {objectType.Name}", nameof(content));
		}
	}
}
