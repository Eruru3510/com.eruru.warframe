using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusSortie {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		public string Boss;
		public string Faction;
		public Variant[] Variants;

		public class Variant {

			public string MissionType;
			public string Modifier;
			[JsonField (typeof (NodeTranslator))]
			public string Node;

		}

	}

}