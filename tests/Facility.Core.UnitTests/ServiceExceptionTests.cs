using System;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests
{
	public class ServiceExceptionTests
	{
		[Test]
		public void BasicTests()
		{
			const string code = "Error";
			const string message = "The error message.";
			const string outerCode = "OuterError";
			const string outerMessage = "The outer error message.";

			try
			{
				throw new ServiceException(new ServiceErrorDto(code, message));
			}
			catch (ServiceException exception)
			{
				exception.Error.Code.Should().Be(code);
				exception.Message.Should().Be(message);

				try
				{
					throw new ServiceException(new ServiceErrorDto(outerCode, outerMessage), exception);
				}
				catch (ServiceException outerException)
				{
					outerException.Error.Code.Should().Be(outerCode);
					outerException.Message.Should().Be(outerMessage);
					outerException.InnerException!.Message.Should().Be(message);
				}
			}
		}

		[Test]
		public void RequireError()
		{
			Assert.Throws<ArgumentNullException>(() => { throw new ServiceException(null!); });
			Assert.Throws<ArgumentNullException>(() => { throw new ServiceException(null!, new ArgumentException()); });
		}
	}
}
