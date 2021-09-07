using System;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests
{
	public sealed class EnumTests
	{
		[Test]
		public void EnumPreservesCase()
		{
			const string unorthodoxAnswerValue = "noNeverAskAgain";
			var unorthodoxAnswer = new Answer(unorthodoxAnswerValue);
			unorthodoxAnswer.ToString().Should().Be(unorthodoxAnswerValue);
		}

		[Test]
		public void DefinedEnumComparisonIsCaseInsensitive()
		{
			var isYes = new Answer("YeS") == Answer.Yes;
			isYes.Should().BeTrue();
		}

		[Test]
		public void UndefinedComparisonIsCaseInsensitive()
		{
			var unorthodoxAnswer = new Answer("noNeverAskAgain");
			var isUnorthodoxAnswer = new Answer("noneveraskagain") == unorthodoxAnswer;
			isUnorthodoxAnswer.Should().BeTrue();
		}

		[Test]
		public void SwitchExpressions()
		{
			var answer = Answer.Yes;

			var invertedAnswer = answer.ToString() switch
			{
				Answer.Strings.Yes => Answer.No,
				Answer.Strings.No => Answer.Yes,
				Answer.Strings.Maybe => Answer.Maybe,
				_ => throw new InvalidOperationException($"Unsupported answer: {answer}"),
			};

			invertedAnswer.Should().Be(Answer.No);
		}

		[Test]
		public void AvoidsCrashOnDefaultEnumToString()
		{
			Func<string> toString = () => default(Answer).ToString();
			toString.Should().NotThrow();
		}
	}
}
