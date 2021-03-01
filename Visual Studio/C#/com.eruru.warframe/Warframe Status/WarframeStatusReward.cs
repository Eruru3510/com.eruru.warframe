using System.Text;
using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeStatusReward {

		[JsonField (typeof (ItemArrayTranslator))]
		public string Items;
		public CountedItem[] CountedItems;
		public int Credits;

		public override string ToString () {
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (Items);
			if (CountedItems?.Length > 0) {
				if (stringBuilder.Length > 0) {
					stringBuilder.Append ('、');
				}
				for (int i = 0; i < CountedItems.Length; i++) {
					if (stringBuilder.Length > 0) {
						stringBuilder.Append ("、");
					}
					stringBuilder.Append (CountedItems[i].Key);
					if (CountedItems[i].Count > 1) {
						stringBuilder.Append ($"({CountedItems[i].Count})");
					}
				}
			}
			if (Credits > 0) {
				if (stringBuilder.Length > 0) {
					stringBuilder.Append ('、');
				}
				stringBuilder.Append ($"现金{Credits}");
			}
			return stringBuilder.ToString ();
		}

		public class CountedItem {

			public int Count;
			[JsonField (typeof (ItemTranslator))]
			public string Key;

		}

	}

}