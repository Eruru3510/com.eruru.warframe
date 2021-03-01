using System;
using Eruru.Json;

namespace com.eruru.warframe {

	class RewardTranslator : IJsonConverter<WarframeStatusReward, string> {

		public string Read (WarframeStatusReward value) {
			return value.ToString ();
		}

		public WarframeStatusReward Write (string value) {
			throw new NotImplementedException ();
		}

	}

}