using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Facility.Core.Http
{
	/// <summary>
	/// Uses JSON to serialize and deserialize DTOs for HTTP requests and responses.
	/// </summary>
	public class JsonHttpContentSerializer : HttpContentSerializer
	{
		/// <summary>
		/// Creates an instance.
		/// </summary>
		public JsonHttpContentSerializer()
		{
			SupportedMediaTypes = new[] { HttpServiceUtility.JsonMediaType };
		}

		/// <summary>
		/// The supported media types. Defaults to "application/json".
		/// </summary>
		public IReadOnlyList<string> SupportedMediaTypes { get; }

		/// <summary>
		/// An instance of JsonHttpContentSerializer.
		/// </summary>
		public static JsonHttpContentSerializer Instance = new JsonHttpContentSerializer();

		/// <summary>
		/// The media type for requests.
		/// </summary>
		protected override string DefaultMediaTypeCore => SupportedMediaTypes?.FirstOrDefault();

		/// <summary>
		/// Determines if the specified media type is supported.
		/// </summary>
		protected override bool IsSupportedMediaTypeCore(string mediaType)
		{
			return SupportedMediaTypes?.Contains(mediaType) ?? false;
		}

		/// <summary>
		/// Creates HTTP content for the specified DTO.
		/// </summary>
		protected override HttpContent CreateHttpContentCore(ServiceDto content, string mediaType)
		{
			return new DelegateHttpContent(mediaType ?? DefaultMediaType, stream => ServiceJsonUtility.ToJsonStream(content, stream));
		}

		/// <summary>
		/// Reads a DTO from the specified HTTP content.
		/// </summary>
		protected override async Task<ServiceResult<ServiceDto>> ReadHttpContentAsyncCore(Type dtoType, HttpContent content, CancellationToken cancellationToken)
		{
			try
			{
				using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
				using (var textReader = new StreamReader(stream))
					return ServiceResult.Success((ServiceDto) ServiceJsonUtility.FromJsonTextReader(textReader, dtoType));
			}
			catch (JsonException exception)
			{
				return ServiceResult.Failure(HttpServiceErrors.CreateInvalidContent(exception.Message));
			}
		}

		private sealed class DelegateHttpContent : HttpContent
		{
			public DelegateHttpContent(string mediaType, Action<Stream> writeToStream)
			{
				m_writeToStream = writeToStream;
				Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
			}

			protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
			{
				m_writeToStream(stream);
				return Task.FromResult<object>(null);
			}

			protected override bool TryComputeLength(out long length)
			{
				length = -1L;
				return false;
			}

			readonly Action<Stream> m_writeToStream;
		}
	}
}
