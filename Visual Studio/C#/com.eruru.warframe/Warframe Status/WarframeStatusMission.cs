using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeStatusMission {

		[JsonField (typeof (NodeTranslator))]
		public string Node;
		public string Type;
		public string Faction;
		public string ExclusiveWeapon;
		public WarframeStatusReward Reward;

	}

}