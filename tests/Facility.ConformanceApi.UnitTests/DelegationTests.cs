using System;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Assertions;
using FluentAssertions;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

namespace Facility.ConformanceApi.UnitTests
{
	public sealed class DelegationTests
	{
		[Test]
		public async Task NotImplemented()
		{
			var api = new DelegatingConformanceApi(ServiceDelegators.NotImplemented);
			Awaiting(async () => await api.CheckQueryAsync(new CheckQueryRequestDto(), CancellationToken.None)).Should().Throw<NotImplementedException>();
		}

		[Test]
		public async Task Override()
		{
			var api = new CheckPathCounter();
			(await api.CheckPathAsync(new CheckPathRequestDto(), CancellationToken.None)).Should().BeSuccess();
			api.Count.Should().Be(1);
		}

		[Test]
		public async Task Forward()
		{
			var inner = new CheckPathCounter();
			var api = new DelegatingConformanceApi(ServiceDelegators.Forward(inner));
			(await api.CheckPathAsync(new CheckPathRequestDto(), CancellationToken.None)).Should().BeSuccess();
			inner.Count.Should().Be(1);
		}

		[Test]
		public async Task CallTwice()
		{
			var inner = new CheckPathCounter();
			var api = new DelegatingConformanceApi(
				async (method, request, cancellationToken) =>
				{
					await method.InvokeAsync(inner, request, cancellationToken);
					return await method.InvokeAsync(inner, request, cancellationToken);
				});
			(await api.CheckPathAsync(new CheckPathRequestDto(), CancellationToken.None)).Should().BeSuccess();
			inner.Count.Should().Be(2);
		}

		[Test]
		public async Task RightResponse()
		{
			var api = new DelegatingConformanceApi(async (_, _, _) => ServiceResult.Success<ServiceDto>(new CheckPathResponseDto()));
			(await api.CheckPathAsync(new CheckPathRequestDto(), CancellationToken.None)).Should().BeSuccess();
		}

		[Test]
		public async Task WrongResponse()
		{
			var api = new DelegatingConformanceApi(async (_, _, _) => ServiceResult.Success<ServiceDto>(new CheckQueryResponseDto()));
			Awaiting(async () => await api.CheckPathAsync(new CheckPathRequestDto(), CancellationToken.None)).Should().Throw<InvalidCastException>();
		}

		private sealed class CheckPathCounter : DelegatingConformanceApi
		{
			public CheckPathCounter()
				: base(ServiceDelegators.NotImplemented)
			{
			}

			public override async Task<ServiceResult<CheckPathResponseDto>> CheckPathAsync(CheckPathRequestDto request, CancellationToken cancellationToken)
			{
				Count++;
				return ServiceResult.Success(new CheckPathResponseDto());
			}

			public int Count { get; private set; }
		}
	}
}
