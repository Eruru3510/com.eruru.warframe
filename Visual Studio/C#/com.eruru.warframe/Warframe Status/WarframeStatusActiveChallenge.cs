using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusActiveChallenge {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		public bool IsDaily;
		public bool IsElite;
		[JsonField ("desc", typeof (ItemTranslator))]
		public string Description;
		[JsonField (typeof (ItemTranslator))]
		public string Title;
		public int Reputation;

	}

}