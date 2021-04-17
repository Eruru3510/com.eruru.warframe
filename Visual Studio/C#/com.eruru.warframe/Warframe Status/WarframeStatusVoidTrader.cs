using System;
using Eruru.Json;
using Eruru.Localizer;

namespace com.eruru.warframe {

	public class WarframeStatusVoidTrader {

		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Expiry;
		[JsonField (typeof (WarframeStatusUpdater))]
		[LocalizerField (typeof (RelativeTimeConverter))]
		public DateTime Activation;
		public bool Active;
		[JsonField (typeof (NodeTranslator))]
		public string Location;
		public WarframeStatusInventory[] Inventory {

			get => _Inventory;

			set {
				var olds = _Inventory;
				_Inventory = value;
				if (value.Length > olds?.Length) {
					Api.BroadcastGroupMessage (WarframeStatus.GetVoidTraderInformation (null));
				}
			}

		}

		WarframeStatusInventory[] _Inventory;

	}

}