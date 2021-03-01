using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusSyndicateMission {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		public string Syndicate;
		public WarframeStatusJob[] Jobs;

	}

}