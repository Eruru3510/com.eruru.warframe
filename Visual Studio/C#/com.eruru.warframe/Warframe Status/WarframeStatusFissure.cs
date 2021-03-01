using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusFissure : IComparable {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		[JsonField (typeof (NodeTranslator))]
		public string Node;
		public string MissionType;
		public string EnemyKey;
		public string Tier;
		public int TierNum;

		public int CompareTo (object obj) {
			WarframeStatusFissure fissure = (WarframeStatusFissure)obj;
			return TierNum.CompareTo (fissure.TierNum);
		}

	}

}