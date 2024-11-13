// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using Facility.Core;
using Facility.Core.Http;

#pragma warning disable 612, 618 // member is obsolete

namespace EdgeCases.Http
{
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public static partial class EdgeCasesHttpMapping
	{
		/// <summary>
		/// An old method.
		/// </summary>
		[Obsolete]
		public static readonly HttpMethodMapping<OldMethodRequestDto, OldMethodResponseDto> OldMethodMapping =
			new HttpMethodMapping<OldMethodRequestDto, OldMethodResponseDto>.Builder
			{
				HttpMethod = new HttpMethod("PATCH"),
				Path = "/oldMethod",
				ResponseMappings =
				{
					new HttpResponseMapping<OldMethodResponseDto>.Builder
					{
						StatusCode = (HttpStatusCode) 200,
					}.Build(),
				},
			}.Build();

		/// <summary>
		/// Custom HTTP method.
		/// </summary>
		public static readonly HttpMethodMapping<CustomHttpRequestDto, CustomHttpResponseDto> CustomHttpMapping =
			new HttpMethodMapping<CustomHttpRequestDto, CustomHttpResponseDto>.Builder
			{
				HttpMethod = HttpMethod.Post,
				Path = "/customHttp",
				RequestBodyType = typeof(CustomHttpRequestDto),
				GetRequestBody = request =>
					new CustomHttpRequestDto
					{
						Value = request.Value,
					},
				CreateRequest = body =>
					new CustomHttpRequestDto
					{
						Value = ((CustomHttpRequestDto?) body)?.Value,
					},
				ResponseMappings =
				{
					new HttpResponseMapping<CustomHttpResponseDto>.Builder
					{
						StatusCode = (HttpStatusCode) 200,
						ResponseBodyType = typeof(CustomHttpResponseDto),
						GetResponseBody = response =>
							new CustomHttpResponseDto
							{
								Value = response.Value,
							},
						CreateResponse = body =>
							new CustomHttpResponseDto
							{
								Value = ((CustomHttpResponseDto) body!).Value,
							},
					}.Build(),
				},
			}.Build();

		public static readonly HttpMethodMapping<SnakeMethodRequestDto, SnakeMethodResponseDto> SnakeMethodMapping =
			new HttpMethodMapping<SnakeMethodRequestDto, SnakeMethodResponseDto>.Builder
			{
				HttpMethod = HttpMethod.Post,
				Path = "/snake_method",
				RequestBodyType = typeof(SnakeMethodRequestDto),
				ResponseMappings =
				{
					new HttpResponseMapping<SnakeMethodResponseDto>.Builder
					{
						StatusCode = (HttpStatusCode) 200,
						ResponseBodyType = typeof(SnakeMethodResponseDto),
					}.Build(),
				},
			}.Build();

		public static readonly HttpMethodMapping<MiscRequestDto, MiscResponseDto> MiscMapping =
			new HttpMethodMapping<MiscRequestDto, MiscResponseDto>.Builder
			{
				HttpMethod = HttpMethod.Post,
				Path = "/misc",
				GetUriParameters = request =>
					new Dictionary<string, string?>
					{
						{ "q", request.Q },
					},
				SetUriParameters = (request, parameters) =>
				{
					parameters.TryGetValue("q", out var queryParameterQ);
					request.Q = queryParameterQ;
					return request;
				},
				RequestBodyType = typeof(MiscRequestDto),
				GetRequestBody = request =>
					new MiscRequestDto
					{
						F = request.F,
					},
				CreateRequest = body =>
					new MiscRequestDto
					{
						F = ((MiscRequestDto?) body)?.F ?? default,
					},
				ResponseMappings =
				{
					new HttpResponseMapping<MiscResponseDto>.Builder
					{
						StatusCode = (HttpStatusCode) 200,
					}.Build(),
				},
			}.Build();
	}
}
