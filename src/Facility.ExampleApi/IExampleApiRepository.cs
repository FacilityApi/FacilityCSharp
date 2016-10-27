using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Facility.ExampleApi
{
	/// <summary>
	/// Example repository for widgets.
	/// </summary>
	public interface IExampleApiRepository
	{
		/// <summary>
		/// Gets widgets.
		/// </summary>
		Task<GetWidgetsResponseDto> GetWidgetsAsync(GetWidgetsRequestDto request, CancellationToken cancellationToken);

		/// <summary>
		/// Creates a new widget.
		/// </summary>
		Task<WidgetDto> CreateWidgetAsync(WidgetDto widget, CancellationToken cancellationToken);

		/// <summary>
		/// Gets the specified widget.
		/// </summary>
		Task<WidgetDto> TryGetWidgetAsync(string id, CancellationToken cancellationToken);

		/// <summary>
		/// Deletes the specified widget.
		/// </summary>
		Task<bool> TryDeleteWidgetAsync(string id, CancellationToken cancellationToken);

		/// <summary>
		/// Gets the specified widgets.
		/// </summary>
		Task<IReadOnlyList<WidgetDto>> GetWidgetBatchAsync(IEnumerable<string> ids, CancellationToken cancellationToken);

		/// <summary>
		/// Edits a widget.
		/// </summary>
		Task<EditWidgetResponseDto> EditWidgetAsync(string id, IEnumerable<JObject> ops, CancellationToken cancellationToken);

		/// <summary>
		/// Gets a widget preference.
		/// </summary>
		Task<PreferenceDto> TryGetPreferenceAsync(string key, CancellationToken cancellationToken);

		/// <summary>
		/// Sets a widget preference.
		/// </summary>
		Task SetPreferenceAsync(string key, PreferenceDto value, CancellationToken cancellationToken);
	}
}
