using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeMarketOrder {

		public int Quantity;
		public bool Visible;
		[JsonField ("mod_rank")]
		public int ModRank;
		public int Platinum;
		[JsonField ("order_type")]
		public WarframeMarketOrderType OrderType;
		public WarframeMarketUser User;

	}

}