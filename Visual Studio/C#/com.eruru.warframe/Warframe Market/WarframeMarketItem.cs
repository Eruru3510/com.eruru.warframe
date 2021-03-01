using System;
using Eruru.Json;

namespace com.eruru.warframe {

	public class WarframeMarketItem : IComparable {

		[JsonField ("url_name")]
		public string UrlName;
		[JsonField ("item_name")]
		public string ItemName {

			get => _ItemName;

			set {
				_ItemName = value;
				Name = TranslateSystem.TranslateItem (value);
			}

		}
		public string Name;

		internal int Index;
		internal string Text;

		string _ItemName;

		public WarframeMarketItem () {

		}
		internal WarframeMarketItem (string urlName, string itemName, string name, int index, string text) {
			UrlName = urlName ?? throw new ArgumentNullException (nameof (urlName));
			ItemName = itemName ?? throw new ArgumentNullException (nameof (itemName));
			Name = name ?? throw new ArgumentNullException (nameof (name));
			Index = index;
			Text = text ?? throw new ArgumentNullException (nameof (text));
		}

		public int CompareTo (object obj) {
			WarframeMarketItem item = (WarframeMarketItem)obj;
			if (Index == item.Index) {
				if (Text.Length == item.Text.Length) {
					return Text.CompareTo (item.Text);
				}
				return Text.Length.CompareTo (item.Text.Length);
			}
			return Index.CompareTo (item.Index);
		}

	}

}