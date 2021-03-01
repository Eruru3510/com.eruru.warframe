using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeMarketUser {

		[JsonField ("ingame_name")]
		public string InGameName;
		public WarframeMarketUserStatus Status;

	}

}