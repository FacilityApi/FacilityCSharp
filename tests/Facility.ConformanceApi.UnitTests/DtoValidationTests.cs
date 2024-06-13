using System.Net;
using Facility.ConformanceApi.Http;
using Facility.Core;
using Facility.Core.Assertions;
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests;

[TestFixtureSource(nameof(ServiceSerializers))]
public sealed class DtoValidationTests : ServiceSerializerTestsBase
{
	public DtoValidationTests(ServiceSerializer serializer)
		: base(serializer)
	{
	}

	[Test]
	public void RequiredRequestFieldsSpecified()
	{
		var dto = CreateRequiredRequest();
		dto.Validate(out var errorMessage).Should().BeTrue();
		errorMessage.Should().BeNull();
	}

	[Test]
	public void RequiredResponseFieldsSpecified()
	{
		var dto = CreateRequiredResponse();
		dto.Validate(out var errorMessage).Should().BeTrue();
		errorMessage.Should().BeNull();
	}

	[Test]
	public async Task HttpRequiredFieldsSpecified()
	{
		var api = CreateHttpApi();
		var dto = CreateRequiredRequest();
		var result = await api.RequiredAsync(dto);
		result.Should().BeSuccess();
	}

	[Test]
	public async Task HttpRequiredFieldsMissingNoValidation()
	{
		var api = CreateHttpApi(skipClientValidation: true, skipServerValidation: true, requiredResponse: new RequiredResponseDto());
		var result = await api.RequiredAsync(new RequiredRequestDto());
		result.Should().BeSuccess();
	}

	[Test]
	public void RequiredNormalRequestFieldMissing()
	{
		var dto = CreateRequiredRequest();
		dto.Normal = null;
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetRequiredFieldErrorMessage("normal"));
	}

	[Test]
	public async Task HttpRequiredNormalRequestFieldMissing([Values] bool skipClientValidation)
	{
		var api = CreateHttpApi(skipClientValidation: skipClientValidation);
		var dto = CreateRequiredRequest();
		dto.Normal = null;
		var result = await api.RequiredAsync(dto);
		result.Should().BeFailure(ServiceErrors.CreateRequestFieldRequired("normal"));
	}

	[Test]
	public void RequiredNormalResponseFieldMissing()
	{
		var dto = CreateRequiredResponse();
		dto.Normal = null;
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetRequiredFieldErrorMessage("normal"));
	}

	[Test]
	public async Task HttpRequiredNormalResponseFieldMissing([Values] bool skipClientValidation)
	{
		var dto = CreateRequiredResponse();
		dto.Normal = null;
		var api = CreateHttpApi(requiredResponse: dto, skipClientValidation: skipClientValidation);
		var result = await api.RequiredAsync(CreateRequiredRequest());
		result.Should().BeFailure(ServiceErrors.CreateResponseFieldRequired("normal"));
	}

	[Test]
	public void RequiredQueryFieldMissing()
	{
		var dto = CreateRequiredRequest();
		dto.Query = null;
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetRequiredFieldErrorMessage("query"));
	}

	[Test]
	public async Task HttpRequiredQueryFieldMissing([Values] bool skipClientValidation)
	{
		var api = CreateHttpApi(skipClientValidation: skipClientValidation);
		var dto = CreateRequiredRequest();
		dto.Query = null;
		var result = await api.RequiredAsync(dto);
		result.Should().BeFailure(ServiceErrors.CreateRequestFieldRequired("query"));
	}

	[Test]
	public void RequiredPathFieldMissing()
	{
		var dto = new GetWidgetRequestDto();
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetRequiredFieldErrorMessage("id"));
	}

	[Test]
	public void RequiredPathFieldSpecified()
	{
		var dto = new GetWidgetRequestDto { Id = 3 };
		dto.Validate(out var errorMessage).Should().BeTrue();
		errorMessage.Should().BeNull();
	}

	[Test]
	public async Task HttpRequiredPathFieldMissing([Values] bool skipClientValidation)
	{
		var api = CreateHttpApi(skipClientValidation: skipClientValidation);
		var result = await api.GetWidgetAsync(new GetWidgetRequestDto());
		result.Should().BeFailure(ServiceErrors.CreateRequestFieldRequired("id"));
	}

	[Test]
	public async Task HttpRequiredPathFieldSpecified()
	{
		var api = CreateHttpApi();
		var result = await api.GetWidgetAsync(new GetWidgetRequestDto { Id = 3 });
		result.Should().BeSuccess();
	}

	[Test]
	public void RequiredBodyFieldMissing()
	{
		var dto = new GetWidgetBatchRequestDto();
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetRequiredFieldErrorMessage("ids"));
	}

	[Test]
	public void RequiredBodyFieldSpecified()
	{
		var dto = new GetWidgetBatchRequestDto { Ids = [3] };
		dto.Validate(out var errorMessage).Should().BeTrue();
		errorMessage.Should().BeNull();
	}

	[Test]
	public async Task HttpRequiredBodyFieldMissing([Values] bool skipClientValidation)
	{
		var api = CreateHttpApi(skipClientValidation: skipClientValidation);
		var result = await api.GetWidgetBatchAsync(new GetWidgetBatchRequestDto());
		result.Should().BeFailure(ServiceErrors.CreateRequestFieldRequired("ids"));
	}

	[Test]
	public async Task HttpRequiredBodyFieldSpecified()
	{
		var api = CreateHttpApi();
		var result = await api.GetWidgetBatchAsync(new GetWidgetBatchRequestDto { Ids = [3] });
		result.Should().BeSuccess();
	}

	[Test]
	public void RequiredWidgetNameMissing()
	{
		var dto = CreateRequiredRequest();
		dto.Widget = new WidgetDto();
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("widget", ServiceDataUtility.GetRequiredFieldErrorMessage("name")));
	}

	[Test]
	public void RequiredWidgetsNameMissing()
	{
		var dto = CreateRequiredRequest();
		dto.Widgets = [CreateWidget(), new WidgetDto()];
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("widgets[1]", ServiceDataUtility.GetRequiredFieldErrorMessage("name")));
	}

	[Test]
	public void RequiredWidgetMatrixNameMissing()
	{
		var dto = CreateRequiredRequest();
		dto.WidgetMatrix = [[new WidgetDto()]];
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("widgetMatrix[0][0]", ServiceDataUtility.GetRequiredFieldErrorMessage("name")));
	}

	[Test]
	public void RequiredWidgetResultNameMissing()
	{
		var dto = CreateRequiredRequest();
		dto.WidgetResult = ServiceResult.Success(new WidgetDto());
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("widgetResult", ServiceDataUtility.GetRequiredFieldErrorMessage("name")));
	}

	[Test]
	public void RequiredWidgetResultsNameMissing()
	{
		var dto = CreateRequiredRequest();
		dto.WidgetResults = [ServiceResult.Success(new WidgetDto())];
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("widgetResults[0]", ServiceDataUtility.GetRequiredFieldErrorMessage("name")));
	}

	[Test]
	public void RequiredWidgetMapNameMissing()
	{
		var dto = CreateRequiredRequest();
		dto.WidgetMap = new Dictionary<string, WidgetDto> { { "key", new WidgetDto() } };
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("widgetMap.key", ServiceDataUtility.GetRequiredFieldErrorMessage("name")));
	}

	[Test]
	public void RequiredHasWidgetNameMissing()
	{
		var dto = CreateRequiredRequest();
		dto.HasWidget = new HasWidgetDto { Widget = new WidgetDto() };
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("hasWidget", ServiceDataUtility.GetInvalidFieldErrorMessage("widget", ServiceDataUtility.GetRequiredFieldErrorMessage("name"))));
	}

	[Test]
	public void ValidateGetWidgetHasPositiveId()
	{
		var dto = new GetWidgetRequestDto { Id = -1 };
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("id", "Must be at least 0."));
	}

	[Test]
	public void ValidateDeleteWidgetHasPositiveId()
	{
		var dto = new DeleteWidgetRequestDto { Id = -1 };
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("id", "Must be at least 0."));
	}

	[Test]
	public void ValidateBatchWidgetFailsExceedingMaximumIds()
	{
		var dto = new GetWidgetBatchRequestDto
		{
			Ids = [1, 2, 3, 4, 50, 200, 300, 500, 700, 1000, 1001],
		};
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("ids", "Count must be at most 10."));
	}

	[Test]
	public void ValidateFailsLessThanCount()
	{
		var dto = CreateRequiredRequest();
		dto.Point = [0.0];
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("point", "Count must be at least 2."));
	}

	[Test]
	public void ValidateFailsMoreThanCount()
	{
		var dto = CreateRequiredRequest();
		dto.Point = [0.0, 1.0, 2.0];
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("point", "Count must be at most 2."));
	}

	[Test]
	public void ValidateWidgetIdIsPositive()
	{
		var dto = CreateWidget();
		dto.Id = -1;
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("id", "Must be at least 0."));
	}

	[Test]
	public void ValidateWidgetNameMatchesPattern()
	{
		var dto = new WidgetDto { Id = 1, Name = "%%widget%%" };
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("name", "Must match regular expression: ^[_a-zA-Z0-9]+$"));
	}

	[Test]
	public void ValidateWidgetNameLength()
	{
		var dto = new WidgetDto { Id = 1, Name = "ExcessivelyLongName" };
		dto.Validate(out var errorMessage).Should().BeFalse();
		errorMessage.Should().Be(ServiceDataUtility.GetInvalidFieldErrorMessage("name", "Length must be at most 10."));
	}

	private static RequiredRequestDto CreateRequiredRequest() => new() { Query = "query", Normal = "normal" };

	private static RequiredResponseDto CreateRequiredResponse() => new() { Normal = "normal" };

	private static WidgetDto CreateWidget() => new() { Name = "name" };

	private HttpClientConformanceApi CreateHttpApi(bool skipClientValidation = false, bool skipServerValidation = false, RequiredResponseDto? requiredResponse = null)
	{
		var service = new FakeConformanceApiService(Serializer, requiredResponse: requiredResponse);
		var contentSerializer = HttpContentSerializer.Create(Serializer);
		var settings = new ServiceHttpHandlerSettings
		{
			SkipRequestValidation = skipServerValidation,
			SkipResponseValidation = skipServerValidation,
			ContentSerializer = contentSerializer,
		};
		var handler = new ConformanceApiHttpHandler(service, settings) { InnerHandler = new NotFoundHttpHandler() };
		var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://example.com/") };
		return new HttpClientConformanceApi(new HttpClientServiceSettings
		{
			HttpClient = httpClient,
			SkipRequestValidation = skipClientValidation,
			SkipResponseValidation = skipClientValidation,
			ContentSerializer = contentSerializer,
		});
	}

	private sealed class FakeConformanceApiService : DelegatingConformanceApi
	{
		public FakeConformanceApiService(ServiceSerializer serializer, RequiredResponseDto? requiredResponse = null, WidgetDto? widgetResponse = null)
			: base(ServiceDelegators.NotImplemented)
		{
			m_serializer = serializer;
			m_requiredResponse = requiredResponse ?? CreateRequiredResponse();
			m_widgetResponse = widgetResponse ?? CreateWidget();
		}

		public override async Task<ServiceResult<GetWidgetResponseDto>> GetWidgetAsync(GetWidgetRequestDto request, CancellationToken cancellationToken = default) =>
			ServiceResult.Success(new GetWidgetResponseDto { Widget = m_serializer.Clone(m_widgetResponse) });

		public override async Task<ServiceResult<GetWidgetBatchResponseDto>> GetWidgetBatchAsync(GetWidgetBatchRequestDto request, CancellationToken cancellationToken = default) =>
			ServiceResult.Success(new GetWidgetBatchResponseDto { Results = [] });

		public override async Task<ServiceResult<RequiredResponseDto>> RequiredAsync(RequiredRequestDto request, CancellationToken cancellationToken = default) =>
			ServiceResult.Success(m_serializer.Clone(m_requiredResponse));

		private readonly ServiceSerializer m_serializer;
		private readonly RequiredResponseDto m_requiredResponse;
		private readonly WidgetDto m_widgetResponse;
	}

	private sealed class NotFoundHttpHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
	}
}
