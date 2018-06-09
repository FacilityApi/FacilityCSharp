using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
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
		protected override bool IsSupportedMediaTypeCore(string mediaType) => SupportedMediaTypes?.Contains(mediaType) ?? false;

		/// <summary>
		/// Creates HTTP content for the specified DTO.
		/// </summary>
		protected override HttpContent CreateHttpContentCore(object content, string mediaType) =>
			new DelegateHttpContent(mediaType ?? DefaultMediaType, stream => ServiceJsonUtility.ToJsonStream(content, stream));

		/// <summary>
		/// Reads a DTO from the specified HTTP content.
		/// </summary>
		protected override async Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type dtoType, HttpContent content, CancellationToken cancellationToken)
		{
			try
			{
				using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
				using (var textReader = new StreamReader(stream))
					return ServiceResult.Success(ServiceJsonUtility.FromJsonTextReader(textReader, dtoType));
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
				Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

				m_memoryStream = new RecyclableMemoryStream(ServiceDataUtility.RecyclableMemoryStreamManager);
				writeToStream(m_memoryStream);
			}

			protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
			{
				m_memoryStream.Position = 0;
				return m_memoryStream.CopyToAsync(stream);
			}

			protected override bool TryComputeLength(out long length)
			{
				length = m_memoryStream.Length;
				return true;
			}

			protected override void Dispose(bool disposing)
			{
				try
				{
					if (disposing)
						m_memoryStream.Dispose();
				}
				finally
				{
					base.Dispose(disposing);
				}
			}

			readonly MemoryStream m_memoryStream;
		}
	}
}
