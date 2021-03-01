using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeStatusJob {

		[JsonField (typeof (BountyItemArrayTranslator))]
		public string RewardPool;
		[JsonField (typeof (ItemTranslator))]
		public string Type;

	}

}