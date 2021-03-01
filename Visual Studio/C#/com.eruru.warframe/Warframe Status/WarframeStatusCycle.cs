using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusCycle {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		public bool IsDay {

			get => _IsDay;

			set {
				_IsDay = value;
				Active = value ? "白昼" : "夜晚";
			}

		}
		public string Active;
		public bool IsWarm {

			get => _IsWarm;

			set {
				_IsWarm = value;
				Active = value ? "温暖" : "寒冷";
			}

		}

		bool _IsDay;
		bool _IsWarm;

	}

}