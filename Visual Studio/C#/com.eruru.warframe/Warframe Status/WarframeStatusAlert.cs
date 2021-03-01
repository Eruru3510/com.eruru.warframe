using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusAlert {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		public WarframeStatusMission Mission;

	}

}