using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;

namespace Facility.Core.UnitTests
{
	public class ServiceErrorDtoTests
	{
		[Test]
		public void ToStringUsesJson()
		{
			s_error.ToString().ShouldBe(@"{""code"":""Test"",""message"":""Message."",""details"":{""Some"":""details.""},""innerError"":{""code"":""Inner""}}");
		}

		[Test]
		public void BasicEquivalence()
		{
			var empty = new ServiceErrorDto();
			var full = s_error;

			empty.IsEquivalentTo(null).ShouldBe(false);
			empty.IsEquivalentTo(empty).ShouldBe(true);
			empty.IsEquivalentTo(new ServiceErrorDto()).ShouldBe(true);
			empty.IsEquivalentTo(full).ShouldBe(false);
			full.IsEquivalentTo(new ServiceErrorDto(s_error.Code)).ShouldBe(false);

			full.IsEquivalentTo(null).ShouldBe(false);
			full.IsEquivalentTo(empty).ShouldBe(false);
			full.IsEquivalentTo(new ServiceErrorDto(s_error.Code, s_error.Message) { Details = s_error.Details, InnerError = s_error.InnerError }).ShouldBe(true);
			full.IsEquivalentTo(full).ShouldBe(true);
			full.IsEquivalentTo(new ServiceErrorDto(s_error.Code)).ShouldBe(false);
		}

		readonly ServiceErrorDto s_error = new ServiceErrorDto("Test", "Message.") { Details = new JObject { ["Some"] = "details." }, InnerError = new ServiceErrorDto("Inner") };
	}
}
