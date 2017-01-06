using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// Serializes and deserializes DTOs for HTTP requests and responses.
	/// </summary>
	public abstract class HttpContentSerializer
	{
		/// <summary>
		/// The default media type for the serializer.
		/// </summary>
		public string DefaultMediaType => DefaultMediaTypeCore;

		/// <summary>
		/// Determines if the specified media type is supported.
		/// </summary>
		public bool IsSupportedMediaType(string mediaType)
		{
			return IsSupportedMediaTypeCore(mediaType);
		}

		/// <summary>
		/// Creates HTTP content for the specified DTO.
		/// </summary>
		public HttpContent CreateHttpContent(object content, string mediaType = null)
		{
			if (mediaType != null && !IsSupportedMediaType(mediaType))
				throw new ArgumentException($"Unsupported media type '{mediaType}'.");

			return CreateHttpContentCore(content, mediaType);
		}

		/// <summary>
		/// Reads a DTO from the specified HTTP content.
		/// </summary>
		public async Task<ServiceResult<T>> ReadHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
			where T : ServiceDto
		{
			return (await ReadHttpContentAsync(typeof(T), content, cancellationToken).ConfigureAwait(false)).Map(x => (T) x);
		}

		/// <summary>
		/// Reads a DTO from the specified HTTP content.
		/// </summary>
		public async Task<ServiceResult<object>> ReadHttpContentAsync(Type dtoType, HttpContent content, CancellationToken cancellationToken = default(CancellationToken))
		{
			var contentType = content?.Headers.ContentType;
			if (contentType == null)
				return ServiceResult.Failure(HttpServiceErrors.CreateMissingContentType());

			string mediaType = contentType.MediaType;
			if (!IsSupportedMediaType(mediaType))
				return ServiceResult.Failure(HttpServiceErrors.CreateUnsupportedContentType(mediaType));

			return await ReadHttpContentAsyncCore(dtoType, content, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// The media type for requests.
		/// </summary>
		protected abstract string DefaultMediaTypeCore { get; }

		/// <summary>
		/// Determines if the specified media type is supported.
		/// </summary>
		protected abstract bool IsSupportedMediaTypeCore(string mediaType);

		/// <summary>
		/// Creates HTTP content for the specified DTO.
		/// </summary>
		protected abstract HttpContent CreateHttpContentCore(object content, string mediaType);

		/// <summary>
		/// Reads a DTO from the specified HTTP content.
		/// </summary>
		protected abstract Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type dtoType, HttpContent content, CancellationToken cancellationToken);
	}
}
