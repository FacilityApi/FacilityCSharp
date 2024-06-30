using System;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Assertions;
using FluentAssertions;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

namespace Facility.ConformanceApi.UnitTests;

public sealed class DelegationTests
{
	[Test]
	public async Task NotImplemented()
	{
		var api = new DelegatingConformanceApi(ServiceDelegates.NotImplemented);
		await Awaiting(async () => await api.CheckQueryAsync(new CheckQueryRequestDto())).Should().ThrowAsync<NotImplementedException>();
	}

	[Test]
	public async Task Override()
	{
		var api = new CheckPathCounter();
		(await api.CheckPathAsync(new CheckPathRequestDto())).Should().BeSuccess();
		api.Count.Should().Be(1);
	}

	[Test]
	public async Task Forward()
	{
		var inner = new CheckPathCounter();
		var api = new DelegatingConformanceApi(ServiceDelegates.Forward(inner));
		(await api.CheckPathAsync(new CheckPathRequestDto())).Should().BeSuccess();
		inner.Count.Should().Be(1);
	}

	[Test]
	public async Task CallTwice()
	{
		var inner = new CheckPathCounter();
		var api = new DelegatingConformanceApi(ServiceDelegate.FromDelegator(
			async (method, request, cancellationToken) =>
			{
				await method.InvokeAsync(inner, request, cancellationToken);
				return await method.InvokeAsync(inner, request, cancellationToken);
			}));
		(await api.CheckPathAsync(new CheckPathRequestDto())).Should().BeSuccess();
		inner.Count.Should().Be(2);
	}

	[Test]
	public async Task RightResponse()
	{
		var api = new DelegatingConformanceApi(ServiceDelegate.FromDelegator(async (_, _, _) => ServiceResult.Success<ServiceDto>(new CheckPathResponseDto())));
		(await api.CheckPathAsync(new CheckPathRequestDto())).Should().BeSuccess();
	}

	[Test]
	public async Task WrongResponse()
	{
		var api = new DelegatingConformanceApi(ServiceDelegate.FromDelegator(async (_, _, _) => ServiceResult.Success<ServiceDto>(new CheckQueryResponseDto())));
		await Awaiting(async () => await api.CheckPathAsync(new CheckPathRequestDto())).Should().ThrowAsync<InvalidCastException>();
	}

	[Test]
	public async Task Validate()
	{
		var invalidWidget = new WidgetDto();
		var validWidget = new WidgetDto { Id = 1, Name = "one" };

		var createdWidget = invalidWidget;
		var api = new DelegatingConformanceApi(ServiceDelegate.FromDelegator(async (_, _, _) => ServiceResult.Success<ServiceDto>(new CreateWidgetResponseDto { Widget = createdWidget })));
		(await api.CreateWidgetAsync(new CreateWidgetRequestDto { Widget = invalidWidget })).Should().BeSuccess();

		var validatingApi = new DelegatingConformanceApi(ServiceDelegates.Validate(api));
		(await validatingApi.CreateWidgetAsync(new CreateWidgetRequestDto { Widget = invalidWidget })).Should().BeFailure(ServiceErrors.InvalidRequest);
		(await validatingApi.CreateWidgetAsync(new CreateWidgetRequestDto { Widget = validWidget })).Should().BeFailure(ServiceErrors.InvalidResponse);

		createdWidget = validWidget;
		(await validatingApi.CreateWidgetAsync(new CreateWidgetRequestDto { Widget = invalidWidget })).Should().BeFailure(ServiceErrors.InvalidRequest);
		(await validatingApi.CreateWidgetAsync(new CreateWidgetRequestDto { Widget = validWidget })).Should().BeSuccess();
	}

	private sealed class CheckPathCounter : DelegatingConformanceApi
	{
		public CheckPathCounter()
			: base(ServiceDelegates.NotImplemented)
		{
		}

		public override async Task<ServiceResult<CheckPathResponseDto>> CheckPathAsync(CheckPathRequestDto request, CancellationToken cancellationToken = default)
		{
			Count++;
			return ServiceResult.Success(new CheckPathResponseDto());
		}

		public int Count { get; private set; }
	}
}
