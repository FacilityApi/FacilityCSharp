using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Newtonsoft.Json.Linq;

#pragma warning disable 1998 // async without await

namespace Facility.ExampleApi.InMemory
{
	public sealed class InMemoryExampleApiRepository : IExampleApiRepository
	{
		public InMemoryExampleApiRepository()
		{
			m_lock = new object();
			m_widgets = new Dictionary<string, WidgetDto>();
			m_preferences = new Dictionary<string, PreferenceDto>();
		}

		public void AddOrUpdateWidgets(IEnumerable<WidgetDto> widgets)
		{
			lock (m_lock)
			{
				foreach (var widget in widgets)
					m_widgets[widget.Id] = widget;
			}
		}

		public async Task<GetWidgetsResponseDto> GetWidgetsAsync(GetWidgetsRequestDto request, CancellationToken cancellationToken)
		{
			IEnumerable<WidgetDto> widgets;

			if (request.Query == null)
			{
				lock (m_lock)
					widgets = m_widgets.Values.ToList();
			}
			else if (request.Query.Length == 0)
			{
				widgets = new WidgetDto[0];
			}
			else
			{
				lock (m_lock)
					widgets = m_widgets.Values.Where(x => x.Name.Contains(request.Query)).ToList();
			}

			int total = widgets.Count();

			Func<WidgetDto, string> keySelector;
			if (request.Sort == WidgetField.Name)
				keySelector = x => x.Name;
			else
				keySelector = x => x.Id;

			widgets = request.Desc.GetValueOrDefault() ? widgets.OrderByDescending(keySelector) : widgets.OrderBy(keySelector);

			if (request.Limit != null)
				widgets = widgets.Take(Math.Max(0, request.Limit ?? 10));

			return new GetWidgetsResponseDto { Widgets = widgets.ToList(), Total = total };
		}

		public async Task<WidgetDto> CreateWidgetAsync(WidgetDto widget, CancellationToken cancellationToken)
		{
			var newWidget = widget != null ? ServiceDataUtility.Clone(widget) : new WidgetDto();
			newWidget.Id = Guid.NewGuid().ToString("N");

			lock (m_lock)
				m_widgets.Add(newWidget.Id, newWidget);

			return newWidget;
		}

		public async Task<WidgetDto> TryGetWidgetAsync(string id, CancellationToken cancellationToken)
		{
			return TryGetWidgetById(id);
		}

		public async Task<bool> TryDeleteWidgetAsync(string id, CancellationToken cancellationToken)
		{
			lock (m_lock)
				return m_widgets.Remove(id);
		}

		public async Task<IReadOnlyList<WidgetDto>> GetWidgetBatchAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
		{
			return ids.Select(TryGetWidgetById).ToList();
		}

		public async Task<EditWidgetResponseDto> EditWidgetAsync(string id, IEnumerable<JObject> ops, CancellationToken cancellationToken)
		{
			if (ops != null && ops.Any())
				return new EditWidgetResponseDto { Job = new WidgetJobDto { Id = "TODO" } };
			else
				return new EditWidgetResponseDto { Widget = TryGetWidgetById(id) };
		}

		public async Task<PreferenceDto> TryGetPreferenceAsync(string key, CancellationToken cancellationToken)
		{
			PreferenceDto preference = null;
			if (!string.IsNullOrEmpty(key))
			{
				lock (m_lock)
					m_preferences.TryGetValue(key, out preference);
			}
			return preference;
		}

		public async Task SetPreferenceAsync(string key, PreferenceDto value, CancellationToken cancellationToken)
		{
			lock (m_lock)
				m_preferences[key] = value;
		}

		public static InMemoryExampleApiRepository CreateWithSampleWidgets()
		{
			var service = new InMemoryExampleApiRepository();
			service.AddOrUpdateWidgets(SampleWidgets);
			return service;
		}

		public static IReadOnlyList<WidgetDto> SampleWidgets =
			new[]
			{
				new WidgetDto { Id = "red", Name = "Reddy" },
				new WidgetDto { Id = "white", Name = "Whitey" },
				new WidgetDto { Id = "blue", Name = "Bluey" },
			};

		private WidgetDto TryGetWidgetById(string id)
		{
			WidgetDto widget;
			lock (m_lock)
				m_widgets.TryGetValue(id, out widget);
			return widget;
		}

		readonly object m_lock;
		readonly Dictionary<string, WidgetDto> m_widgets;
		readonly Dictionary<string, PreferenceDto> m_preferences;
	}
}
