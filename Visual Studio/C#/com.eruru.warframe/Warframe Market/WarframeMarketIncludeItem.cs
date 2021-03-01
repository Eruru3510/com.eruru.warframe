using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeMarketIncludeItem {

		[JsonField ("items_in_set")]
		public WarframeMarketItemsInSet[] ItemsInSet;

	}

}