using System;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests;

internal sealed class EnumTests
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
		var toString = () => default(Answer).ToString();
		toString.Should().NotThrow();
	}

	[Test]
	public void Inequality()
	{
		(Answer.Yes == Answer.No).Should().BeFalse();
		(Answer.Yes != Answer.No).Should().BeTrue();
	}

	[Test]
	public void DefaultEnumIsEmpty()
	{
		var def = default(Answer);
		var empty = new Answer("");
		(def == empty).Should().BeTrue();
		(def != empty).Should().BeFalse();
		def.GetHashCode().Should().Be(empty.GetHashCode());
	}

	[Test]
	public void GetValues()
	{
		Answer.GetValues().Should().Equal(Answer.Yes, Answer.No, Answer.Maybe);
	}

	[Test]
	public void AvoidsCrashOnDefaultIsDefined()
	{
		var isDefined = () => default(Answer).IsDefined();
		isDefined.Should().NotThrow();
	}
}
