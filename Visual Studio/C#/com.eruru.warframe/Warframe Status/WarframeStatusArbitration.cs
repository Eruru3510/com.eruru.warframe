using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusArbitration {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		[JsonField (typeof (NodeTranslator))]
		public string Node;
		public string Enemy;
		public string Type;

	}

}