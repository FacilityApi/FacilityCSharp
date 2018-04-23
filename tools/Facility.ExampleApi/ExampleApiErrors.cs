using Facility.Core;

namespace Facility.ExampleApi
{
	public static partial class ExampleApiErrors
	{
		public static ServiceErrorDto CreateInvalidRequestMissingWidgetIds()
		{
			return ServiceErrors.CreateInvalidRequest("Must specify at least one widget ID.");
		}

		public static ServiceErrorDto CreateInvalidRequestMissingWidgetId()
		{
			return ServiceErrors.CreateInvalidRequest("The widget ID is missing.");
		}

		public static ServiceErrorDto CreateNotFoundWidget(string id)
		{
			return ServiceErrors.CreateNotFound($"The widget with ID '{id}' was not found.");
		}
	}
}
