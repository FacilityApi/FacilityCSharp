using Facility.Core;

namespace Facility.ExampleApi
{
	public sealed partial class WidgetDto : ServiceDto<WidgetDto>
	{
		public WidgetDto(string id = null, string name = null)
		{
			Id = id;
			Name = name;
		}
	}
}
