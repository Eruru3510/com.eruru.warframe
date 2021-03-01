using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusDailyDeal {

		[JsonField (typeof (ItemTranslator))]
		public string Item;
		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		public int OriginalPrice;
		public int SalePrice;
		public int Total;
		public int Sold;
		public int Discount;

	}

}