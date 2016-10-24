using System;
using Shouldly;
using Xunit;

namespace Facility.Core.UnitTests
{
	public class ServiceExceptionTests
	{
		[Fact]
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
				exception.Error.Code.ShouldBe(code);
				exception.Message.ShouldBe(message);

				try
				{
					throw new ServiceException(new ServiceErrorDto(outerCode, outerMessage), exception);
				}
				catch (ServiceException outerException)
				{
					outerException.Error.Code.ShouldBe(outerCode);
					outerException.Message.ShouldBe(outerMessage);
					outerException.InnerException.Message.ShouldBe(message);
				}
			}
		}

		[Fact]
		public void RequireError()
		{
			Assert.Throws<ArgumentNullException>((Action) (() => { throw new ServiceException(null); }));
			Assert.Throws<ArgumentNullException>((Action) (() => { throw new ServiceException(null, new ArgumentException()); }));
		}
	}
}
