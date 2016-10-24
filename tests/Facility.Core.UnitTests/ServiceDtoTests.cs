using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Shouldly;
using Xunit;

namespace Facility.Core.UnitTests
{
	public class ServiceDtoTests
	{
		[Fact]
		public void ToStringUsesJson()
		{
			var dto = new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } };
			dto.ToString().ShouldBe(@"{""id"":3,""name"":""Three"",""children"":[{""name"":""child""}]}");
		}

		[Fact]
		public void BasicEquivalence()
		{
			var empty = new TestDto();
			var full = new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } };

			empty.IsEquivalentTo(null).ShouldBe(false);
			empty.IsEquivalentTo(empty).ShouldBe(true);
			empty.IsEquivalentTo(new TestDto()).ShouldBe(true);
			empty.IsEquivalentTo(full).ShouldBe(false);
			full.IsEquivalentTo(new TestDto { Id = 3 }).ShouldBe(false);

			full.IsEquivalentTo(null).ShouldBe(false);
			full.IsEquivalentTo(empty).ShouldBe(false);
			full.IsEquivalentTo(new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } }).ShouldBe(true);
			full.IsEquivalentTo(full).ShouldBe(true);
			full.IsEquivalentTo(new TestDto { Id = 3 }).ShouldBe(false);
		}

		[Fact]
		public void BasicCloning()
		{
			var first = new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } };
			var second = ServiceDataUtility.Clone(first);
			first.IsEquivalentTo(second).ShouldBe(true);
			second.Id += 1;
			first.IsEquivalentTo(second).ShouldBe(false);
		}

		[SuppressMessage("ReSharper", "All", Justification = "unit test")]
		private class TestDto : ServiceDto<TestDto>
		{
			public int? Id { get; set; }

			public string Name { get; set; }

			public IReadOnlyList<TestDto> Children { get; set; }

			public override bool IsEquivalentTo(TestDto other)
			{
				return other != null &&
					other.Id == Id &&
					other.Name == Name &&
					ServiceDataUtility.AreEquivalentArrays(other.Children, Children, ServiceDataUtility.AreEquivalentDtos);
			}
		}
	}
}
