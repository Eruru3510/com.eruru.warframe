using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeStatusInventory {

		[JsonField (typeof (ItemTranslator))]
		public string Item;
		public int Ducats;
		public int Credits;

	}

}